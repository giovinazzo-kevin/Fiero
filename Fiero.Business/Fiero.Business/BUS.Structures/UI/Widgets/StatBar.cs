using Fiero.Core;

using SFML.Graphics;

namespace Fiero.Business.BUS.Structures.UI.Widgets
{
    [TransientDependency]
    public class StatBar : Widget
    {
        public readonly UIControlProperty<int> Value = new(nameof(Value));
        public readonly UIControlProperty<int> MaxValue = new(nameof(MaxValue));
        public readonly UIControlProperty<string> Stat = new(nameof(Stat));
        public readonly UIControlProperty<ColorName> Color = new(nameof(Color));

        protected readonly LayoutRef<Label> StatLabel = new();
        protected readonly LayoutRef<Label> ValueLabel = new();
        protected readonly LayoutRef<ProgressBar> ProgressBar = new();

        public StatBar(GameUI ui)
            : base(ui)
        {
            Stat.ValueUpdated += Stat_ValueChanged;
            Value.ValueUpdated += Value_ValueChanged;
            MaxValue.ValueUpdated += MaxValue_ValueChanged;
        }

        private void MaxValue_ValueChanged(UIControlProperty<int> arg1, int arg2) => Invalidate();
        private void Value_ValueChanged(UIControlProperty<int> arg1, int arg2) => Invalidate();
        private void Stat_ValueChanged(UIControlProperty<string> arg1, string arg2) => Invalidate();

        protected override LayoutThemeBuilder DefineStyles(LayoutThemeBuilder builder) => base.DefineStyles(builder)
            .Style<Label>(r => r
                .Apply(l => l.VerticalAlignment.V = VerticalAlignment.Middle)
                .Apply(l => l.HorizontalAlignment.V = HorizontalAlignment.Center)
                .Apply(l => l.Origin.V = new Vec(0, 1)))
            .Style<Label>(h => h
                .Match(x => x.Id == "stat-label")
                .Apply(l => l.Foreground.V = UI.GetColor(Color))
                .Apply(l => l.Margin.V = new(l.FontSize.V.X, 0)))
            .Style<ProgressBar>(r => r
                .Apply(p => p.Foreground.V = UI.GetColor(Color))
                .Apply(l => l.Background.V = SFML.Graphics.Color.Transparent)
                .Apply(p => p.HorizontalAlignment.V = HorizontalAlignment.Left)
                .Apply(p => p.VerticalAlignment.V = VerticalAlignment.Top)
                .Apply(p => p.Scale.V = new(1, 1))
            );

        protected override LayoutGrid RenderContent(LayoutGrid grid) => grid
            .Col(w: 32, px: true, id: "stat-label")
                .Cell(StatLabel)
            .End()
            .Col()
                .Cell(ProgressBar)
                .Cell(ValueLabel)
            .End();

        protected void Invalidate()
        {
            if (!IsOpen)
                return;
            ProgressBar.Control.Progress.V = MaxValue.V != 0 ? Value.V / (float)MaxValue.V : 0;
            ValueLabel.Control.Text.V = $"{Value.V}/{MaxValue.V}";
            StatLabel.Control.Text.V = $"{Stat.V}:";
        }
        protected override void DefaultSize() { }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);
        }
    }
}
