using Fiero.Core;

namespace Fiero.Business
{
    // TODO: Make UIControl
    public class StatBar : Widget
    {
        public readonly string Stat;
        public readonly ColorName Color;

        public readonly UIControlProperty<int> Value = new(nameof(Value));
        public readonly UIControlProperty<int> MaxValue = new(nameof(MaxValue));
        public readonly Label Label;

        public StatBar(GameUI ui, string stat, ColorName color)
            : base(ui)
        {
            Stat = stat;
            Color = color;
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<UIControl>(r => r.Apply(p => p.Scale.V = new(2, 2)))
            .AddRule<ProgressBar>(r => r.Apply(p => p.Foreground.V = UI.GetColor(Color)))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid grid) => grid
            .Row()
                .Cell<ProgressBar>(p =>
                {
                    p.Length.V = 0;
                    Value.ValueChanged += (__, _) => p.Invalidate();
                    MaxValue.ValueChanged += (__, _) => p.Invalidate();

                    p.Invalidated += _ =>
                    {
                        p.Progress.V = MaxValue.V != 0 ? Value.V / (float)MaxValue.V : (float)0;
                    };
                })
            .End();

        public override void Draw()
        {
            base.Draw();
        }
    }
}
