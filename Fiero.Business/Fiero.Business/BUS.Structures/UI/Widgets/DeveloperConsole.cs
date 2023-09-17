using Ergo.Shell;
using Fiero.Core;

using System;
using System.Text;
using System.Threading;
using Unconcern;
using Unconcern.Common;

namespace Fiero.Business
{

    [TransientDependency]
    public partial class DeveloperConsole : Widget
    {
        public const double ScriptUpdateRate = 0.15;
        public const int TabSize = 2;

        protected readonly GameColors<ColorName> Colors;
        protected ConsolePane Pane { get; private set; }

        public readonly EventBus EventBus;
        public readonly ErgoScriptingSystem ScriptingSystem;

        public event Action<DeveloperConsole, string> OutputAvailable;
        public event Action<DeveloperConsole, char> CharAvailable;
        public event Action<DeveloperConsole, string> LineAvailable;

        public DeveloperConsole(ErgoScriptingSystem scripting, EventBus bus, GameUI ui, GameColors<ColorName> colors)
            : base(ui)
        {
            EventBus = bus;
            Colors = colors;
            ScriptingSystem = scripting;
            OutputAvailable += OnOutputAvailable;
            EnableDragging = false;
            Data.UI.ViewportSize.ValueChanged += ViewportSize_ValueChanged;
            void ViewportSize_ValueChanged(GameDatumChangedEventArgs<Coord> obj)
            {
                if (Layout != null)
                    Layout.Size.V = obj.NewValue;
            }
        }

        protected override void DefaultSize()
        {
            Layout.Size.V = UI.Store.Get(Data.UI.ViewportSize);
        }

        protected void WriteLine(string s)
        {
            Pane.WriteLine(s);
            LineAvailable?.Invoke(this, s);
        }

        protected void Put(char c)
        {
            CharAvailable?.Invoke(this, c);
        }

        public void Show()
        {
            Layout.IsHidden.V = false;
            Layout.Focus(Pane);
        }

        public void Hide()
        {
            Layout.IsHidden.V = true;
            Pane.IsActive.V = false;
        }

        protected virtual void OnOutputAvailable(DeveloperConsole self, string chunk)
        {
            if (Layout is null)
                return;
            Pane.Write(chunk);
        }

        public Subscription TrackShell(ErgoShell shell)
        {
            shell.UseColors = false;
            shell.UseANSIEscapeSequences = false;
            shell.UseUnicodeSymbols = false;
            var cts = new CancellationTokenSource();
            var outExpr = Concern.Defer()
                .UseAsynchronousTimer()
                //.After(TimeSpan.FromMilliseconds(50))
                .Do(async token =>
                {
                    var sb = new StringBuilder();
                    var result = await ScriptingSystem.Out.Reader.ReadAsync(token);
                    var buffer = result.Buffer;
                    foreach (var segment in buffer)
                    {
                        var bytes = segment.Span.ToArray();
                        var str = ScriptingSystem.OutWriter.Encoding.GetString(bytes);
                        sb.Append(str);
                    }
                    ScriptingSystem.Out.Reader.AdvanceTo(buffer.End);
                    OutputAvailable?.Invoke(this, sb.ToString());
                })
                .Build();
            var replExpr = Concern.Defer()
                .UseAsynchronousTimer()
                .After(TimeSpan.FromMilliseconds(50))
                .Do(async token =>
                {
                    await foreach (var ans in shell.Repl(ScriptingSystem.StdlibScope, ct: token)) ;
                })
                .Build();
            _ = Concern.Deferral.LoopForever(outExpr, cts.Token);
            _ = Concern.Deferral.LoopForever(replExpr, cts.Token);
            var closure = new ShellClosure(shell, ScriptingSystem.InWriter);
            CharAvailable += closure.OnCharAvailable;
            LineAvailable += closure.OnLineAvailable;
            return new(new[] { () => {
                CharAvailable -= closure.OnCharAvailable;
                LineAvailable -= closure.OnLineAvailable;
                cts.Cancel();
                ScriptingSystem.In.Reader.CancelPendingRead();
                ScriptingSystem.Out.Reader.CancelPendingRead();
            } });
        }

        protected override LayoutThemeBuilder DefineStyles(LayoutThemeBuilder builder) => base.DefineStyles(builder)
            .Style<ConsolePane>(r => r.Apply(p =>
            {
                var ts = UI.Store.Get(Data.UI.TileSize);
                p.Background.V = Colors.Get(ColorName.Black).AddAlpha(-64);
                p.Foreground.V = Colors.Get(ColorName.White);
                p.Margin.V = p.Padding.V = new(ts, ts);
            }))
            .Style<UIControl>(r => r.Apply(x =>
            {
                x.BorderColor.V = Colors.Get(ColorName.White).AddAlpha(-64);
                x.OutlineThickness.V = 1;
            }))
            ;

        protected override void OnLayoutRebuilt(Layout oldValue)
        {
            base.OnLayoutRebuilt(oldValue);
            Layout.Focus(Pane);
        }

        protected override LayoutGrid RenderContent(LayoutGrid grid) => grid
                .Row(id: "output")
                    .Cell<ConsolePane>(p =>
                    {
                        Pane = p;
                        Pane.CenterContentV.V = Pane.CenterContentH.V = false;
                        Pane.Caret.CharAvailable += (caret, ch) =>
                        {
                            if (ScriptingSystem.Shell.InputReader.Blocking)
                            {
                                Put(ch);
                            }
                        };
                        Pane.Caret.EnterPressed += (caret) =>
                        {
                            if (!ScriptingSystem.Shell.InputReader.Blocking)
                            {
                                Pane.History.Add(Pane.Caret.Text);
                                WriteLine(Pane.Caret.Text);
                                Pane.ScrollToCursor();
                            }
                        };
                    })
                .End()
            ;
    }
}
