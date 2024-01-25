namespace Fiero.Core
{
    [TransientDependency]
    public class SimpleToolTip : ToolTip
    {
        private string _text = string.Empty;
        public readonly LayoutRef<Paragraph> Paragraph = new();

        public SimpleToolTip SetText(string text)
        {
            _text = text;
            if (Paragraph.Control?.Text is { } ctrl)
                ctrl.V = text;
            return this;
        }

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

        protected override LayoutThemeBuilder DefineStyles(LayoutThemeBuilder builder) => base.DefineStyles(builder)
            .Rule<Paragraph>(b => b
                .Apply(x => x.Padding.V = new(8, 8)));

        protected override LayoutGrid RenderContent(LayoutGrid grid)
        {
            return grid
                .Row(@class: "tooltip")
                    .Cell(Paragraph, p => p.Text.V = _text)
                .End()
            ;
        }
    }
}
