using SFML.Graphics;

namespace Fiero.Core
{
    public class Checkbox : UIControl
    {
        public readonly UIControlProperty<bool> Checked = new(nameof(Checked));

        public Checkbox(GameInput input) : base(input)
        {
            Clickable.V = true;
        }

        protected override void OnClicked(Coord mousePos)
        {
            Checked.V = !Checked.V;
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            base.Draw(target, states);
            if(Checked.V) {
                var rect = new RectangleShape(BorderRenderSize.ToVector2f() / 2) {
                    Position = (BorderRenderPos + BorderRenderSize / 4).ToVector2f(),
                    FillColor = Accent
                };
                target.Draw(rect, states);
            }
        }
    }
}
