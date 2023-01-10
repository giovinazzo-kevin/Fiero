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
                if (old != null)
                {
                    old.Log.LogAdded -= LogAdded;
                }
                if (f.V != null)
                {
                    f.V.Log.LogAdded += LogAdded;
                }

                void LogAdded(LogComponent component, string newLog)
                {
                    var labels = Layout.Query(x => true, x => x.HasClass("log-row"))
                        .Cast<Label>();
                    var messages = component.GetMessages().TakeLast(NumRowsDisplayed.V - 1).Append(newLog);
                    foreach (var (l, m) in labels.Zip(messages))
                    {
                        l.Text.V = m;
                    }
                }
            };
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<Label>(r => r.Apply(p => p.CenterContentH.V = false))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid grid) => grid
                .Repeat(NumRowsDisplayed.V, (i, g) => g
                    .Row(@class: "log-row", id: $"log-row-{i}")
                        .Cell<Label>()
                    .End()
                )
            ;
    }
}
