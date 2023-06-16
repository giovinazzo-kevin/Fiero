using SFML.Graphics;

namespace Fiero.Core
{
    public class ProgressBar : UIControl
    {
        protected readonly Sprite LeftEmpty, MiddleEmpty, RightEmpty;
        protected readonly Sprite LeftHalf, MiddleHalf, RightHalf;
        protected readonly Sprite LeftFull, MiddleFull, RightFull;
        public readonly int TileSize;
        public readonly UIControlProperty<float> Progress = new(nameof(Progress), 0);
        public readonly UIControlProperty<HorizontalAlignment> HorizontalAlignment = new(nameof(HorizontalAlignment), Core.HorizontalAlignment.Left);
        public readonly UIControlProperty<VerticalAlignment> VerticalAlignment = new(nameof(VerticalAlignment), Core.VerticalAlignment.Middle);
        public readonly UIControlProperty<bool> Capped = new(nameof(Capped), true);

        public ProgressBar(GameInput input, int tileSize,
            Sprite le, Sprite me, Sprite re,
            Sprite lh, Sprite mh, Sprite rh,
            Sprite lf, Sprite mf, Sprite rf)
            : base(input)
        {
            TileSize = tileSize;
            LeftEmpty = le; MiddleEmpty = me; RightEmpty = re;
            LeftHalf = lh; MiddleHalf = mh; RightHalf = rh;
            LeftFull = lf; MiddleFull = mf; RightFull = rf;
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            var len = (int)Math.Round(Size.V.X / (TileSize * Scale.V.X));
            base.Draw(target, states);
            for (var i = 0; i < len; i++)
            {
                var full = i < len * Progress;
                var half = (i + 1) > len * Progress;
                var piece = full ? half ? MiddleHalf : MiddleFull : MiddleEmpty;
                if (Capped.V && i == 0)
                {
                    piece = full ? half ? LeftHalf : LeftFull : LeftEmpty;
                }
                else if (Capped.V && i == len - 1)
                {
                    piece = full ? half ? RightHalf : RightFull : RightEmpty;
                }
                piece.Color = Foreground;
                piece.Scale = Scale.V;
                piece.Position = new(ContentRenderPos.X + i * TileSize * Scale.V.X, ContentRenderPos.Y);
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
                //piece.Origin = new Coord(TileSize / 2, 0) - Origin.V * new Coord(TileSize, TileSize);
                target.Draw(piece, states);
            }
        }

        public override void Dispose()
        {
            // No need to dispose the sprites as they are not clones but references to global sprites
        }
    }
}
