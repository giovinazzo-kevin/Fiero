namespace Fiero.Core
{
    [TransientDependency]
    public class SimpleToolTip : ToolTip
    {
        public readonly LayoutRef<Paragraph> Paragraph = new();

        public void SetText(string text) => Paragraph.Control.Text.V = text;

        public SimpleToolTip(GameUI ui) : base(ui)
        {
            Paragraph.ControlChanged += (_, old) =>
            {
                if (old != null)
                    old.Text.ValueChanged -= OnValueChanged;
                if (Paragraph.Control != null)
                    Paragraph.Control.Text.ValueChanged += OnValueChanged;
                void OnValueChanged(UIControlProperty<string> _, string __)
                {
                    Layout.Size.V = Paragraph.Control.MinimumContentSize;
                }
            };
        }

        protected override void DefaultSize() { }

        protected override LayoutGrid RenderContent(LayoutGrid grid)
        {
            return grid
                .Row(@class: "tooltip")
                    .Cell(Paragraph)
                .End()
            ;
        }
    }
}
