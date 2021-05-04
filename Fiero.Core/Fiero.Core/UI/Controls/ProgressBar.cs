using LightInject;
using SFML.Graphics;
using System;

namespace Fiero.Core
{

    public class ProgressBar : UIControl
    {
        public readonly int Length, TileSize;

        private float _progress;
        public event Action<ProgressBar, float> ProgressChanged;
        public float Progress {
            get => _progress;
            set {
                var oldValue = _progress;
                if(_progress != value) {
                    _progress = value;
                    ProgressChanged?.Invoke(this, oldValue);
                }
            }
        }

        public bool Center { get; set; }

        protected readonly Sprite LeftEmpty, MiddleEmpty, RightEmpty;
        protected readonly Sprite LeftHalf, MiddleHalf, RightHalf;
        protected readonly Sprite LeftFull, MiddleFull, RightFull;


        public ProgressBar(GameInput input, int tileSize, int length,
            Sprite le, Sprite me, Sprite re,
            Sprite lh, Sprite mh, Sprite rh,
            Sprite lf, Sprite mf, Sprite rf)
            : base(input)
        {
            Length = length;
            TileSize = tileSize;
            LeftEmpty = le; MiddleEmpty = me; RightEmpty = re;
            LeftHalf = lh; MiddleHalf = mh; RightHalf = rh;
            LeftFull = lf; MiddleFull = mf; RightFull = rf;
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
                piece.Color = IsActive ? ActiveColor : InactiveColor;
                piece.Scale = new(Scale.X, Scale.Y);
                if(Center) {
                    piece.Position = new(Position.X * Scale.X + i * TileSize * Scale.X - (Length / 4 * TileSize * Scale.X), Position.Y * Scale.Y);
                }
                else {
                    piece.Position = new(Position.X * Scale.X + i * TileSize * Scale.X, Position.Y * Scale.Y);
                }
                piece.Origin = new(TileSize / 2, 0);
                target.Draw(piece, states);
            }
        }
    }
}
