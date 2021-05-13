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

        public int Length {
            get => Size.V.X / TileSize;
            set {
                Size.V = new(value * TileSize, TileSize);
            }
        }

        public readonly UIControlProperty<float> Progress = new(nameof(Progress), 0);
        public readonly UIControlProperty<bool> Center = new(nameof(Center), false);

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
            Length = 3;
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;
            base.Draw(target, states);
            for (var i = 0; i < Length; i++) {
                var full = i < Length * Progress;
                var half = (i + 1) > Length * Progress;
                var piece = full ? half ? MiddleHalf : MiddleFull : MiddleEmpty;
                if (i == 0) {
                    piece = full ? half ? LeftHalf : LeftFull : LeftEmpty;
                }
                else if (i == Length - 1) {
                    piece = full ? half ? RightHalf : RightFull : RightEmpty;
                }
                piece.Color = Foreground;
                piece.Scale = Scale.V;
                piece.Position = Center
                    ? (new(Position.V.X * Scale.V.X + i * TileSize * Scale.V.X - (Length / 4 * TileSize * Scale.V.X), Position.V.Y * Scale.V.Y))
                    : (new(Position.V.X * Scale.V.X + i * TileSize * Scale.V.X, Position.V.Y * Scale.V.Y));
                piece.Origin = new(TileSize / 2, 0);
                target.Draw(piece, states);
            }
        }
    }
}
