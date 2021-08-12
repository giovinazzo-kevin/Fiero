using LightInject;
using SFML.Graphics;
using System;

namespace Fiero.Core
{
    public class ProgressBar : UIControl
    {
        protected readonly Sprite LeftEmpty, MiddleEmpty, RightEmpty;
        protected readonly Sprite LeftHalf, MiddleHalf, RightHalf;
        protected readonly Sprite LeftFull, MiddleFull, RightFull;
        public readonly int TileSize;

        public readonly UIControlProperty<int> Length = new(nameof(Length), 3);
        public readonly UIControlProperty<float> Progress = new(nameof(Progress), 0);
        public readonly UIControlProperty<bool> Center = new(nameof(Center), false);
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
            Size.V = new((Length - 1) * TileSize, TileSize);
            base.Draw(target, states);
            for (var i = 0; i < Length; i++) {
                var full = i < Length * Progress;
                var half = (i + 1) > Length * Progress;
                var piece = full ? half ? MiddleHalf : MiddleFull : MiddleEmpty;
                if (Capped.V && i == 0) {
                    piece = full ? half ? LeftHalf : LeftFull : LeftEmpty;
                }
                else if (Capped.V && i == Length - 1) {
                    piece = full ? half ? RightHalf : RightFull : RightEmpty;
                }
                piece.Color = Foreground;
                piece.Scale = Scale.V;
                piece.Position = new(ContentRenderPos.X + i * TileSize * Scale.V.X, ContentRenderPos.Y);
                if(Center.V) {
                    piece.Position += new Coord(1, 0) * (piece.GetLocalBounds().Size() * Length.V - Size.V) / 2;
                }
                piece.Origin = new Coord(TileSize / 2, 0) - Origin.V * new Coord(TileSize, TileSize);
                target.Draw(piece, states);
            }
        }
    }
}
