using Ergo.Lang.Extensions;
using Fiero.Core;
using System;
using System.Linq;
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

        public event Action<ConsoleBox, Script.Stdout> StdoutReceived;

        public ConsoleBox(EventBus bus, GameUI ui, GameColors<ColorName> colors) : base(ui)
        {
            EventBus = bus;
            Colors = colors;
            Rows.ValueChanged += (_, __) => RebuildLayout();
            Cols.ValueChanged += (_, __) => RebuildLayout();
            StdoutReceived += OnStdoutReceived;
        }

        protected virtual void OnStdoutReceived(ConsoleBox self, Script.Stdout msg)
        {
            var paragraph = Layout.Query(x => true, x => "text".Equals(x.Id))
                .Cast<Paragraph>()
                    .Single();
            paragraph.Text.V = (paragraph.Text.V + msg.Message)
                .Split("\n")
                .Select(s => s.Take(Cols.V).Join(string.Empty))
                .TakeLast(Rows.V)
                .Join("\n");
        }

        public Subscription TrackScript(Script s)
        {
            var cts = new CancellationTokenSource();
            var expr = Concern.Defer()
                .UseAsynchronousTimer()
                .Do(async token =>
                {
                    var item = await s.ScriptProperties.Stdout.PullOneAsync(token);
                    StdoutReceived?.Invoke(this, item);
                })
                .Build();
            _ = Concern.Deferral.LoopForever(expr, cts.Token);
            return new(new[] { () => cts.Cancel() });
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<Paragraph>(r => r.Apply(p =>
            {
                var ts = UI.Store.Get(Data.UI.TileSize);
                p.CenterContentH.V = false;
                p.Background.V = Colors.Get(ColorName.White).AddAlpha(-128);
                p.Foreground.V = Colors.Get(ColorName.Black);
                p.Padding.V = new(ts, ts);
                p.Margin.V = new(ts, ts);
            }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid grid) => grid
                .Row(id: "text")
                    .Cell<Paragraph>(p =>
                    {
                        p.Cols.V = Cols.V;
                        p.Rows.V = Rows.V;
                    })
                .End()
            ;
    }
}
