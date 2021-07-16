using SFML.Graphics;
using System;
using System.Collections.Generic;

namespace Fiero.Core
{
    public class GameFonts<TFonts>
        where TFonts : struct, Enum
    {
        protected readonly Dictionary<TFonts, BitmapFont> Fonts;

        public GameFonts()
        {
            Fonts = new Dictionary<TFonts, BitmapFont>();
        }

        public void Add(TFonts key, Coord fontSize, Texture tex)
        {
            var sprites = new Sprite[256];
            for (int i = 0; i < 256; i++) {
                sprites[i] = new(tex, new((i % 16) * fontSize.X, (i / 16) * fontSize.Y, fontSize.X, fontSize.Y));
            }
            Fonts[key] = new(fontSize, sprites);
        }

        public BitmapFont Get(TFonts key) => Fonts.GetValueOrDefault(key);
    }
}
