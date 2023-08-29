using SFML.Graphics;

namespace Fiero.Core
{
    public class ProgressBar : UIControl
    {
        protected readonly Sprite LeftFrame, MiddleFrame, RightFrame;
        protected readonly Sprite Fill;

        public readonly UIControlProperty<float> Progress = new(nameof(Progress), 0);
        public readonly UIControlProperty<HorizontalAlignment> HorizontalAlignment = new(nameof(HorizontalAlignment), Core.HorizontalAlignment.Left);
        public readonly UIControlProperty<VerticalAlignment> VerticalAlignment = new(nameof(VerticalAlignment), Core.VerticalAlignment.Middle);

        public ProgressBar(GameInput input,
            Sprite le, Sprite me, Sprite re, Sprite f)
            : base(input)
        {
            LeftFrame = le; MiddleFrame = me; RightFrame = re;
            Fill = f;
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            var tileSize = MiddleFrame.TextureRect.Size().X;
            var len = (int)(Size.V.X / (tileSize * Scale.V.X));
            base.Draw(target, states);
            for (var i = 0; i < len; i++)
            {
                var piece = true switch
                {
                    _ when i == 0 => LeftFrame,
                    _ when i == len - 1 => RightFrame,
                    _ => MiddleFrame
                };
                piece.Color = Color.White;
                piece.Scale = Scale.V;
                piece.Position = new(ContentRenderPos.X + i * tileSize * Scale.V.X, ContentRenderPos.Y);
                var delta = new Vec(Size.V.X / len, Size.V.Y) - piece.GetLocalBounds().Size();
                switch (HorizontalAlignment.V)
                {
                    case Core.HorizontalAlignment.Center:
                        piece.Position += Coord.PositiveX * delta / 2;
                        break;
                    case Core.HorizontalAlignment.Right:
                        piece.Position += Coord.PositiveX * delta;
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
                Fill.Scale = piece.Scale;
                Fill.Position = piece.Position;
                Fill.Color = Foreground;
                if (piece == MiddleFrame)
                {
                    var full = Progress.V * (len - 2) >= i;
                    var empty = Progress.V * (len - 2) <= i - 1;
                    if (full)
                    {
                        Fill.TextureRect = new(
                            Fill.TextureRect.Left,
                            Fill.TextureRect.Top,
                            tileSize, tileSize
                        );
                        target.Draw(Fill, states);
                    }
                    else if (!empty)
                    {
                        var subTilePercentage = (Progress.V * (len - 2) - (i - 1)) * tileSize;
                        Fill.TextureRect = new(
                            Fill.TextureRect.Left,
                            Fill.TextureRect.Top,
                            (int)subTilePercentage, tileSize
                        );
                        target.Draw(Fill, states);
                    }
                }
                target.Draw(piece, states);
            }
        }

        public override void Dispose()
        {
            // No need to dispose the sprites as they are not clones but references to global sprites
        }
    }
}
