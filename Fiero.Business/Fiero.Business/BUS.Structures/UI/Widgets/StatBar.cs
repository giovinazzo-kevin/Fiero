using Fiero.Core;

namespace Fiero.Business.BUS.Structures.UI.Widgets
{
    [TransientDependency]
    public class StatBar : Widget
    {
        public readonly UIControlProperty<int> Value = new(nameof(Value));
        public readonly UIControlProperty<int> MaxValue = new(nameof(MaxValue));
        public readonly UIControlProperty<string> Stat = new(nameof(Stat));
        public readonly UIControlProperty<ColorName> Color = new(nameof(Color));

        protected Label StatLabel { get; private set; }
        protected Label ValueLabel { get; private set; }
        protected ProgressBar ProgressBar { get; private set; }

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

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<Label>(r => r
                .Apply(l => l.VerticalAlignment.V = VerticalAlignment.Middle)
                .Apply(l => l.HorizontalAlignment.V = HorizontalAlignment.Center)
                .Apply(l => l.Origin.V = new Vec(0, 1)))
            .AddRule<Label>(h => h
                .Match(x => x.Id == "stat-label")
                .Apply(l => l.Foreground.V = UI.GetColor(Color)))
            .AddRule<ProgressBar>(r => r
                .Apply(p => p.Foreground.V = UI.GetColor(Color))
                .Apply(l => l.Background.V = SFML.Graphics.Color.Transparent)
                .Apply(p => p.HorizontalAlignment.V = HorizontalAlignment.Left)
                .Apply(p => p.VerticalAlignment.V = VerticalAlignment.Top)
                .Apply(p => p.Scale.V = new(1, 1))
            );

        protected override LayoutGrid RenderContent(LayoutGrid grid) => grid
            .Col(w: 32, px: true, id: "stat-label")
                .Cell<Label>(l =>
                {
                    StatLabel = l;
                    StatLabel.Margin.V = new(StatLabel.FontSize.V.X / 2 * 2, 0);
                })
            .End()
            .Col()
                .Cell<ProgressBar>(p =>
                {
                    ProgressBar = p;
                })
                .Cell<Label>(l =>
                {
                    ValueLabel = l;
                })
            .End();

        protected void Invalidate()
        {
            if (!IsOpen)
                return;
            ProgressBar.Progress.V = MaxValue.V != 0 ? Value.V / (float)MaxValue.V : 0;
            ValueLabel.Text.V = $"{Value.V}/{MaxValue.V}";
            StatLabel.Text.V = $"{Stat.V}:";
        }

        public override void Draw()
        {
            base.Draw();
        }
    }
}
