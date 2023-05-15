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
        public IntRect GetLocalBounds()
        {
            var global = GetGlobalBounds();
            return new(0, 0, global.Width, global.Height);
        }

        public BitmapText(BitmapFont font, string text)
        {
            Font = font;
            Text = text;
        }

        public void Draw(RenderTarget target, RenderStates states)
        {
            lock (Sprites)
            {
                var bounds = GetLocalBounds().Size();
                if (bounds.X == 0 || bounds.Y == 0)
                    return;

                using var tex = new RenderTexture((uint)bounds.X, (uint)bounds.Y);
                tex.Clear(Color.Transparent);
                foreach (var sprite in Sprites)
                {
                    sprite.Color = FillColor;
                    tex.Draw(sprite);
                }
                tex.Display();
                using var texSprite = new Sprite(tex.Texture);
                texSprite.Scale = Scale;
                var delta = texSprite.Texture.Size.ToVec() * Scale - texSprite.Texture.Size.ToVec();
                texSprite.Position = Position - delta / 2;
                target.Draw(texSprite, states);
            }
        }
    }
}
