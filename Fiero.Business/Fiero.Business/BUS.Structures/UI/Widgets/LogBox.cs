using Fiero.Core;
using System.Linq;

namespace Fiero.Business
{

    [TransientDependency]
    public class LogBox : Widget
    {
        protected readonly GameColors<ColorName> Colors;
        public readonly UIControlProperty<Actor> Following = new(nameof(Following), null);

        public readonly UIControlProperty<int> NumRowsDisplayed = new(nameof(NumRowsDisplayed), 10);

        public LogBox(GameUI ui, GameColors<ColorName> colors)
            : base(ui)
        {
            Colors = colors;
            NumRowsDisplayed.ValueChanged += (_, __) => RebuildLayout();
            Following.ValueUpdated += Following_ValueUpdated;
        }

        private void Following_ValueUpdated(UIControlProperty<Actor> prop, Actor old)
        {
            if (old?.Log != null)
            {
                old.Log.LogAdded -= LogAdded;
            }
            if (prop.V != null)
            {
                prop.V.Log.LogAdded += LogAdded;
                LogAdded(prop.V.Log, string.Empty); // Refresh p text
            }
            void LogAdded(LogComponent component, string _)
            {
                var paragraph = Layout?.Query(x => true, x => "log-text".Equals(x.Id))
                    .Cast<Paragraph>()
                    .SingleOrDefault();
                if (paragraph == null) return;
                var messages = component.GetMessages().TakeLast(NumRowsDisplayed.V);
                paragraph.Text.V = string.Join("\n", messages);
            }
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
