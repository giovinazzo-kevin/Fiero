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
    public class ConsoleBox : Widget
    {
        public const double ScriptUpdateRate = 0.5;

        protected readonly GameColors<ColorName> Colors;
        public readonly EventBus EventBus;
        public readonly UIControlProperty<int> Cols = new(nameof(Cols), 80);
        public readonly UIControlProperty<int> Rows = new(nameof(Rows), 20);

        private DelayedDebounce _delay = new(TimeSpan.FromSeconds(ScriptUpdateRate), 1);
        public event Action<ConsoleBox, string> OutputAvailable;
        public event Action<ConsoleBox, string> InputAvailable;

        public ConsoleBox(EventBus bus, GameUI ui, GameColors<ColorName> colors) : base(ui)
        {
            EventBus = bus;
            Colors = colors;
            Rows.ValueChanged += (_, __) => RebuildLayout();
            Cols.ValueChanged += (_, __) => RebuildLayout();
            OutputAvailable += OnOutputAvailable;
        }

        public void Write(string s)
        {
            InputAvailable?.Invoke(this, s);
        }

        public void WritLine(string s)
        {
            Write(s + Environment.NewLine);
        }

        protected virtual void OnOutputAvailable(ConsoleBox self, string chunk)
        {
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
                paragraph.Text.V = (paragraph.Text.V + chunk)
                    .Split("\n")
                    .Select(s => s.Take(Cols.V).Join(string.Empty))
                    .TakeLast(Rows.V)
                    .Join("\n");
            }
        }
        public Subscription TrackScript(Script s)
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
            InputAvailable += OnInputAvailable;

            return new(new[] { () => {
                cts.Cancel();
                outPipe.Reader.Complete();
                outPipe.Writer.Complete();
                InputAvailable -= OnInputAvailable;
            } });

            void OnInputAvailable(ConsoleBox arg1, string arg2)
            {
                s.ScriptProperties.In.Write(arg2);
                s.ScriptProperties.In.Flush();
            }
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<Paragraph>(r => r.Apply(p =>
            {
                var ts = UI.Store.Get(Data.UI.TileSize);
                p.CenterContentH.V = false;
                p.Background.V = Colors.Get(ColorName.Black).AddAlpha(-128);
                p.Foreground.V = Colors.Get(ColorName.White);
                p.Padding.V = new(ts, ts);
                p.Margin.V = new(ts, ts);
            }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid grid) => grid
                .Row(id: "output")
                    .Cell<Paragraph>(p =>
                    {
                        p.Cols.V = Cols.V;
                        p.Rows.V = Rows.V;
                    })
                .End()
                .Row(id: "input")
                    .Cell<Label>(l =>
                    {
                    })
                .End()
            ;
    }
}
