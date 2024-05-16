using Fiero.Core.Ergo;
using System.Text;
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
        public readonly ErgoScriptHost Host;

        public event Action<DeveloperConsole, string> OutputAvailable;
        public event Action<DeveloperConsole, char> CharAvailable;
        public event Action<DeveloperConsole, string> LineAvailable;

        public DeveloperConsole(EventBus bus, GameUI ui, GameColors<ColorName> colors, IScriptHost host)
            : base(ui)
        {
            EventBus = bus;
            Colors = colors;
            OutputAvailable += OnOutputAvailable;
            EnableDragging = false;
            if (host is not ErgoScriptHost ergoHost)
                throw new NotSupportedException();
            Host = ergoHost;
            Data.View.ViewportSize.ValueChanged += ViewportSize_ValueChanged;
            void ViewportSize_ValueChanged(GameDatumChangedEventArgs<Coord> obj)
            {
                if (Layout != null)
                    Layout.Size.V = obj.NewValue;
            }
        }

        protected override void DefaultSize()
        {
            Layout.Size.V = UI.Store.Get(Data.View.ViewportSize);
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
            {
                var opened = default(Action<UIWindow>);
                opened = w =>
                {
                    Opened -= opened;
                    Pane.Write(chunk);
                };
                Opened += opened;
                return;
            }
            Pane.Write(chunk);
        }

        public Subscription TrackShell()
        {
            Host.Shell.UseColors = false;
            Host.Shell.UseANSIEscapeSequences = false;
            Host.Shell.UseUnicodeSymbols = false;
            var cts = new CancellationTokenSource();
            var outExpr = Concern.Defer()
                //.After(TimeSpan.FromMilliseconds(20))
                .UseAsynchronousTimer()
                .Do(async token =>
                {
                    var sb = new StringBuilder();
                    var result = await Host.Out.Reader.ReadAsync(token);
                    var buffer = result.Buffer;
                    foreach (var segment in buffer)
                    {
                        var bytes = segment.Span.ToArray();
                        var str = Host.OutWriter.Encoding.GetString(bytes);
                        sb.Append(str);
                    }
                    Host.Out.Reader.AdvanceTo(buffer.End);
                    OutputAvailable?.Invoke(this, sb.ToString());
                })
                .Build();
            var replExpr = Concern.Defer()
                .Do(async token =>
                {
                    await foreach (var ans in Host.Shell.Repl(Host.CoreScope, ct: token))
                        ;
                })
                .Build();
            _ = Concern.Deferral.LoopForever(outExpr, cts.Token);
            _ = Concern.Deferral.Once(replExpr, cts.Token);
            var closure = new ShellClosure(Host.Shell, Host.InWriter);
            CharAvailable += closure.OnCharAvailable;
            LineAvailable += closure.OnLineAvailable;
            return new(new[] { () => {
                CharAvailable -= closure.OnCharAvailable;
                LineAvailable -= closure.OnLineAvailable;
                cts.Cancel();
                Host.In.Reader.CancelPendingRead();
                Host.Out.Reader.CancelPendingRead();
            } });
        }

        protected override LayoutThemeBuilder DefineStyles(LayoutThemeBuilder builder) => base.DefineStyles(builder)
            .Rule<ConsolePane>(r => r.Apply(p =>
            {
                var ts = UI.Store.Get(Data.View.TileSize);
                p.Background.V = Colors.Get(ColorName.Black).AddAlpha(-64);
                p.Foreground.V = Colors.Get(ColorName.White);
                p.Margin.V = p.Padding.V = new(ts, ts);
            }))
            .Rule<UIControl>(r => r.Apply(x =>
            {
                x.OutlineColor.V = Colors.Get(ColorName.White).AddAlpha(-64);
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
                            if (Host.Shell.InputReader.Blocking)
                            {
                                Put(ch);
                            }
                        };
                        Pane.Caret.EnterPressed += (caret) =>
                        {
                            if (!Host.Shell.InputReader.Blocking)
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
