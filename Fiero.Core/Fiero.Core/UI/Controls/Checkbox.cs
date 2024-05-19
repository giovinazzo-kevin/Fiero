using SFML.Graphics;
using SFML.Window;

namespace Fiero.Core
{
    public class Checkbox : UIControl
    {
        public UIControlProperty<bool> Checked { get; private set; } = new(nameof(Checked));

        public Checkbox(GameInput input) : base(input)
        {
            IsInteractive.V = true;
        }

        protected override bool OnClicked(Coord mousePos, Mouse.Button button)
        {
            Checked.V = !Checked.V;
            return true;
        }

        protected override void Repaint(RenderTarget target, RenderStates states)
        {
            base.Repaint(target, states);
            if (Checked.V)
            {
                using var rect = new RectangleShape(BorderRenderSize.ToVector2f() / 2)
                {
                    Position = (BorderRenderPos + BorderRenderSize / 4).ToVector2f(),
                    FillColor = Accent.V,
                    OutlineColor = Foreground.V,
                    OutlineThickness = 2f
                };
                target.Draw(rect, states);
            }
            else
            {
                using var rect = new RectangleShape(BorderRenderSize.ToVector2f() / 2)
                {
                    Position = (BorderRenderPos + BorderRenderSize / 4).ToVector2f(),
                    FillColor = Background.V,
                    OutlineColor = Foreground.V,
                    OutlineThickness = 2f
                };
                target.Draw(rect, states);
            }
        }
    }
}
