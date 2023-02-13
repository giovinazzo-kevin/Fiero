using SFML.Graphics;
using System.Collections.Generic;

namespace Fiero.Core
{
    public class BitmapText : Drawable
    {
        private List<Sprite> Sprites = new();

        public readonly BitmapFont Font;

        private string _text;
        public string Text
        {
            get => _text;
            set
            {
                if (!string.Equals(value, _text))
                {
                    _text = value;
                    lock (Sprites)
                    {
                        foreach (var sprite in Sprites)
                        {
                            sprite.Dispose();
                        }
                        Sprites.Clear();
                        Sprites.AddRange(Font.Write(_text));
                    }
                }
            }
        }
        public Coord Position { get; set; }
        public Vec Scale { get; set; } = new(1, 1);
        public Color FillColor { get; set; } = Color.White;

        public IntRect GetGlobalBounds() => new(Position.X, Position.Y, Font.Size.X * _text.Length, Font.Size.Y);
        public IntRect GetLocalBounds() => new(0, 0, Font.Size.X * _text.Length, Font.Size.Y);

        public BitmapText(BitmapFont font, string text)
        {
            Font = font;
            Text = text;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            lock (Sprites)
            {
                foreach (var sprite in Sprites)
                {
                    sprite.Scale = Scale;
                    sprite.Color = FillColor;
                    sprite.Position += Position;
                    target.Draw(sprite);
                    sprite.Position -= Position;
                }
            }
        }
    }
}
