using SFML.Graphics;
using SFML.System;
using System;

namespace Fiero.Core
{
    public class Frame : UIControl
    {
        public readonly int TileSize;
        public readonly Sprite[] Sprites;

        public Frame(GameInput input, int tileSize, Sprite[] sprites)
            : base(input)
        {
            TileSize = tileSize;
            Sprites = sprites;

            Size = new Coord(TileSize, TileSize);
            Position = new Coord(0, 0);
            ActiveColor = Color.White;
        }

        public override void Draw(RenderTarget target, RenderStates states)
        {
            if (IsHidden)
                return;

            for (var i = 0; i < Sprites.Length; i++) {
                if (Sprites[i] == null)
                    continue;
                Sprites[i].Scale = new(Scale.X, Scale.Y);
                Sprites[i].Color = IsActive ? ActiveColor : InactiveColor;
            }
            // Draw top left corner
            if (Sprites[0] != null) {
                Sprites[0].Position = new((Position.X - TileSize) * Scale.X, (Position.Y - TileSize) * Scale.Y);
                target.Draw(Sprites[0], states);
            }
            // Draw top right corner
            if (Sprites[2] != null) {
                Sprites[2].Position = new((Position.X + Size.X) * Scale.X, (Position.Y - TileSize) * Scale.Y);
                target.Draw(Sprites[2], states);
            }
            // Draw bottom left corner
            if (Sprites[6] != null) {
                Sprites[6].Position = new((Position.X - TileSize) * Scale.X, (Position.Y + Size.Y) * Scale.Y);
                target.Draw(Sprites[6], states);
            }
            // Draw bottom right corner
            if (Sprites[8] != null) {
                Sprites[8].Position = new((Position.X + Size.X) * Scale.X, (Position.Y + Size.Y) * Scale.Y);
                target.Draw(Sprites[8], states);
            }
            for (var i = 0; i < Size.X / TileSize; i++) {
                // Draw upper and lower borders
                if (Sprites[1] != null) {
                    Sprites[1].Position = new((Position.X + i * TileSize) * Scale.X, (Position.Y - TileSize) * Scale.Y);
                    target.Draw(Sprites[1], states);
                }
                if (Sprites[7] != null) {
                    Sprites[7].Position = new((Position.X + i * TileSize) * Scale.X, (Position.Y + Size.Y) * Scale.Y);
                    target.Draw(Sprites[7], states);
                }
            }
            for (var i = 0; i < Size.Y / TileSize; i++) {
                // Draw left and right borders
                if (Sprites[3] != null) {
                    Sprites[3].Position = new((Position.X - TileSize) * Scale.X, (Position.Y + i * TileSize) * Scale.Y);
                    target.Draw(Sprites[3], states);
                }
                if (Sprites[5] != null) {
                    Sprites[5].Position = new((Position.X + Size.X) * Scale.X, (Position.Y + i * TileSize) * Scale.Y);
                    target.Draw(Sprites[5], states);
                }

                if (Sprites[4] != null) {
                    for (var j = 0; j < Size.X / TileSize; j++) {
                        // Draw middle
                        Sprites[4].Position = new((Position.X + j * TileSize) * Scale.X, (Position.Y + i * TileSize) * Scale.Y);
                        target.Draw(Sprites[4], states);
                    }
                }
            }
            // Draw children
            base.Draw(target, states);
        }
    }
}
