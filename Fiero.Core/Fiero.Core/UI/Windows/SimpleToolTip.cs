namespace Fiero.Core
{
    [TransientDependency]
    public class SimpleToolTip : ToolTip
    {
        public readonly LayoutRef<Label> Label = new();

        public void SetText(string text) => Label.Control.Text.V = text;

        public SimpleToolTip(GameUI ui) : base(ui)
        {
            Label.ControlChanged += (_, old) =>
            {
                if (old != null)
                    old.Text.ValueChanged -= OnValueChanged;
                if (Label.Control != null)
                    Label.Control.Text.ValueChanged += OnValueChanged;
                void OnValueChanged(UIControlProperty<string> _, string __)
                {
                    Layout.Size.V = Label.Control.MinimumContentSize;
                }
            };
        }

        protected override void DefaultSize() { }

        protected override LayoutGrid RenderContent(LayoutGrid grid)
        {
            return grid
                .Row(@class: "tooltip")
                    .Cell(Label)
                .End()
            ;
        }
    }
}
