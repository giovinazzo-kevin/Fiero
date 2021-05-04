using SFML.Graphics;
using System;
using System.Collections.Generic;

namespace Fiero.Core
{
    public class GameFonts<TFonts>
        where TFonts : struct, Enum
    {
        protected readonly Dictionary<TFonts, Font> Fonts;

        public GameFonts()
        {
            Fonts = new Dictionary<TFonts, Font>();
        }

        public void Add(TFonts key, Font value) => Fonts[key] = value;
        public Font Get(TFonts key) => Fonts.GetValueOrDefault(key);
    }
}
