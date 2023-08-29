using Fiero.Core;
using System.Linq;

namespace Fiero.Business
{

    [TransientDependency]
    public class LogBox : Widget
    {
        protected readonly GameColors<ColorName> Colors;
        public readonly UIControlProperty<Actor> Following = new(nameof(Following), null);

        public Paragraph Paragraph { get; private set; }

        public int NumRowsDisplayed => Layout.ContentRenderSize.Y / (Paragraph?.FontSize.V.Y ?? 12);

        public LogBox(GameUI ui, GameColors<ColorName> colors)
            : base(ui)
        {
            Colors = colors;
            Following.ValueUpdated += Following_ValueUpdated;
        }

        private void UpdateParagraph(LogComponent component)
        {
            if (Paragraph == null) return;
            var messages = component.GetMessages().TakeLast(NumRowsDisplayed);
            Paragraph.Rows.V = NumRowsDisplayed;
            Paragraph.Text.V = string.Join("\n", messages);
        }

        private void Following_ValueUpdated(UIControlProperty<Actor> prop, Actor old)
        {
            if (prop.V == old) return;

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
                UpdateParagraph(component);
            }
        }
        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<Paragraph>(r => r.Apply(p =>
            {
                var ts = UI.Store.Get(Data.UI.TileSize);
                p.CenterContentH.V = false;
                p.Background.V = Colors.Get(ColorName.UIBackground);
                p.Padding.V = new(ts, ts);
            }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid grid) => grid
                .Row(id: "log-text")
                    .Cell<Paragraph>(v => Paragraph = v)
                .End()
            ;

        protected override void OnLayoutRebuilt(Layout oldValue)
        {
            if (oldValue != null)
            {
                oldValue.Size.ValueChanged -= Size_ValueChanged;
            }
            UpdateParagraph(Following.V?.Log);
            Layout.Size.ValueChanged += Size_ValueChanged;
            void Size_ValueChanged(UIControlProperty<Coord> arg1, Coord arg2)
            {
                UpdateParagraph(Following.V?.Log);
            }
        }
    }
}
