using SFML.Graphics;

namespace Fiero.Core
{
    public class Header : Label
    {
        protected readonly Sprite LeftFrame, MiddleFrame, RightFrame;

        public Header(GameInput input,
            Sprite le, Sprite me, Sprite re)
            : base(input)
        {
            LeftFrame = le; MiddleFrame = me; RightFrame = re;
        }

        protected override void Render(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            var bg = Background.V;
            Background.V = Color.Transparent;
            var tileSize = MiddleFrame.TextureRect.Size().X;
            var len = (int)(Size.V.X / (tileSize * Scale.V.X));
            for (var i = 0; i < len; i++)
            {
                var piece = true switch
                {
                    _ when i == 0 => LeftFrame,
                    _ when i == len - 1 => RightFrame,
                    _ => MiddleFrame
                };
                piece.Color = bg;
                piece.Scale = Scale.V;
                piece.Position = new(ContentRenderPos.X + i * tileSize * Scale.V.X, ContentRenderPos.Y);
                var delta = new Vec(Size.V.X / len, Size.V.Y) - piece.GetLocalBounds().Size();
                switch (HorizontalAlignment.V)
                {
                    case Core.HorizontalAlignment.Left:
                        piece.Position += Coord.NegativeX * tileSize / 2;
                        break;
                    case Core.HorizontalAlignment.Right:
                        piece.Position += Coord.PositiveX * tileSize / 2 + Coord.PositiveX * delta;
                        break;
                }
                switch (VerticalAlignment.V)
                {
                    case Core.VerticalAlignment.Middle:
                        piece.Position += Coord.PositiveY * delta / 2;
                        break;
                    case Core.VerticalAlignment.Bottom:
                        piece.Position += Coord.PositiveY * delta;
                        break;
                }
                target.Draw(piece, states);
            }
            base.Render(target, states);
            Background.V = bg;
        }

        public override void Dispose()
        {
            // No need to dispose the sprites as they are not clones but references to global sprites
        }
    }
}
