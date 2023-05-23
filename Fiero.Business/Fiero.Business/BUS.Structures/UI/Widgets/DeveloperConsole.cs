using Ergo.Lang.Extensions;
using Ergo.Shell;
using Fiero.Business.Utils;
using Fiero.Core;
using System;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using Unconcern;
using Unconcern.Common;

namespace Fiero.Business
{
    [TransientDependency]
    public class DeveloperConsole : Widget
    {
        record class ScriptClosure(Script s)
        {
            public void OnInputAvailable(DeveloperConsole _, string arg2)
            {
                s.ScriptProperties.In.Write(arg2);
                s.ScriptProperties.In.Flush();
            }
        };

        public const double ScriptUpdateRate = 0.15;

        private DelayedDebounce _delay = new(TimeSpan.FromSeconds(ScriptUpdateRate), 1);
        private readonly StringBuilder _outputBuffer = new();

        protected readonly GameColors<ColorName> Colors;
        protected Textbox InputTextbox { get; private set; }

        public readonly EventBus EventBus;
        public readonly ErgoScriptingSystem ScriptingSystem;
        public event Action<DeveloperConsole, string> OutputAvailable;
        public event Action<DeveloperConsole, string> InputAvailable;

        public DeveloperConsole(ErgoScriptingSystem scripting, EventBus bus, GameUI ui, GameColors<ColorName> colors)
            : base(ui)
        {
            EventBus = bus;
            Colors = colors;
            ScriptingSystem = scripting;
            OutputAvailable += OnOutputAvailable;
        }

        public void Write(string s)
        {
            InputAvailable?.Invoke(this, s);
            ScriptingSystem.InputAvailable.Raise(new(s));
        }

        public void WriteLine(string s)
        {
            Write(s + Environment.NewLine);
        }

        public void Show()
        {
            Layout.IsHidden.V = false;
            Layout.Focus(InputTextbox);
        }

        public void Hide()
        {
            Layout.IsHidden.V = true;
            InputTextbox.IsActive.V = false;
        }

        protected virtual void OnOutputAvailable(DeveloperConsole self, string chunk)
        {
            _outputBuffer.Append(chunk
                .Replace("⊤", "true")
                .Replace("⊥", "false"));
            if (!_delay.IsDebouncing)
            {
                _delay.Fire += _delay_Fire;
            }
            _delay.Hit();
            void _delay_Fire(Debounce obj)
            {
                obj.Fire -= _delay_Fire;
                var paragraph = Layout.Query(x => true, x => "output".Equals(x.Id))
                    .Cast<Paragraph>()
                        .Single();
                paragraph.Text.V = (paragraph.Text.V + _outputBuffer.ToString())
                    .Replace("\r", string.Empty)
                    .Split('\n')
                    .TakeLast(paragraph.Rows.V)
                    .Join("\n");
                _outputBuffer.Clear();
            }
        }

        public Subscription TrackScript(Script s, bool routeStdin = false)
        {
            var outPipe = new Pipe();
            var inPipe = new Pipe();

            var outWriter = TextWriter.Synchronized(new StreamWriter(outPipe.Writer.AsStream(), ErgoShell.Encoding));
            var outReader = TextReader.Synchronized(new StreamReader(outPipe.Reader.AsStream(), ErgoShell.Encoding));
            var inWriter = TextWriter.Synchronized(new StreamWriter(inPipe.Writer.AsStream(), ErgoShell.Encoding));
            var inReader = TextReader.Synchronized(new StreamReader(inPipe.Reader.AsStream(), ErgoShell.Encoding));

            s.ScriptProperties.In = inWriter;
            s.ScriptProperties.Out = outReader;

            s.ScriptProperties.Solver.SetIn(inReader);
            s.ScriptProperties.Solver.SetOut(outWriter);


            var cts = new CancellationTokenSource();
            var expr = Concern.Defer()
                .UseAsynchronousTimer()
                .Do(async token =>
                {
                    var sb = new StringBuilder();
                    var result = await outPipe.Reader.ReadAsync(token);
                    var buffer = result.Buffer;
                    foreach (var segment in buffer)
                    {
                        var bytes = segment.Span.ToArray();
                        var str = outWriter.Encoding.GetString(bytes);
                        sb.Append(str);
                    }
                    outPipe.Reader.AdvanceTo(buffer.End);
                    OutputAvailable?.Invoke(this, sb.ToString());
                })
                .Build();

            _ = Concern.Deferral.LoopForever(expr, cts.Token);
            var closure = new ScriptClosure(s);
            if (routeStdin) InputAvailable += closure.OnInputAvailable;
            return new(new[] { () => {
                cts.Cancel();
                outPipe.Reader.Complete();
                outPipe.Writer.Complete();
                if(routeStdin) InputAvailable -= closure.OnInputAvailable;
            } });
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<Paragraph>(r => r.Apply(p =>
            {
                var ts = UI.Store.Get(Data.UI.TileSize);
                p.Background.V = Colors.Get(ColorName.Black).AddAlpha(-64);
                p.Foreground.V = Colors.Get(ColorName.White);
                p.Padding.V = new(ts, ts);
                p.Cols.V = p.ContentRenderSize.X / p.FontSize.V.X;
                p.Rows.V = p.ContentRenderSize.Y / p.FontSize.V.Y;
            }))
            .AddRule<Textbox>(r => r.Apply(t =>
            {
                var ts = UI.Store.Get(Data.UI.TileSize);
                t.Padding.V = new(ts / 2, 0);
                t.MaxLength.V = t.ContentRenderSize.X / t.FontSize.V.X;
                t.Background.V = Colors.Get(ColorName.Black).AddAlpha(-128);
                t.Foreground.V = Colors.Get(ColorName.White);
            }))
            .AddRule<UIControl>(r => r.Apply(x =>
            {
                x.OutlineColor.V = Colors.Get(ColorName.White).AddAlpha(-64);
                x.OutlineThickness.V = 1;
            }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid grid) => grid
                .Row(id: "output")
                    .Cell<Paragraph>()
                .End()
                .Row(h: 20, px: true, id: "input")
                    .Cell<Textbox>(t =>
                    {
                        InputTextbox = t;
                        t.EnterPressed += obj =>
                        {
                            var text = obj.Text.V;
                            obj.Text.V = string.Empty;
                            WriteLine(text);
                        };
                    })
                .End()
            ;

        public override void Update()
        {
            base.Update();
        }
    }
}
