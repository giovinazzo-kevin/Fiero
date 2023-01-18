using Fiero.Core;
using System.Linq;

namespace Fiero.Business
{
    public class LogBox : Widget
    {
        protected readonly GameColors<ColorName> Colors;
        public readonly UIControlProperty<Actor> Following = new(nameof(Following), null);

        public readonly UIControlProperty<int> NumRowsDisplayed = new(nameof(NumRowsDisplayed), 10);

        public LogBox(GameUI ui, GameColors<ColorName> colors) : base(ui)
        {
            Colors = colors;
            NumRowsDisplayed.ValueChanged += (_, __) => RebuildLayout();
            Following.ValueChanged += (f, old) =>
            {
                if (old?.Log != null)
                {
                    old.Log.LogAdded -= LogAdded;
                }
                if (f.V != null)
                {
                    f.V.Log.LogAdded += LogAdded;
                }

                void LogAdded(LogComponent component, string newLog)
                {
                    var paragraph = Layout.Query(x => true, x => "log-text".Equals(x.Id))
                        .Cast<Paragraph>()
                        .Single();
                    var messages = component.GetMessages().TakeLast(NumRowsDisplayed.V - 1).Append(newLog);
                    paragraph.Text.V = string.Join("\n", messages);
                }
            };
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<Paragraph>(r => r.Apply(p =>
            {
                var ts = UI.Store.Get(Data.UI.TileSize);
                p.CenterContentH.V = false;
                p.Background.V = Colors.Get(ColorName.UIBackground).AddAlpha(-128);
                p.Padding.V = new(ts, ts);
                p.Margin.V = new(ts, ts);
            }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid grid) => grid
                .Row(id: "log-text")
                    .Cell<Paragraph>()
                .End()
            ;
    }
}
