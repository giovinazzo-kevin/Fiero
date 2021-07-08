using SFML.Graphics;
using System;

namespace Fiero.Core
{
    public static class ColorExtensions
    {
        public static Color AddRgb(this Color c, int r, int g, int b)
        {
            return new(
                (byte)Math.Clamp(c.R + r, 0, 255), 
                (byte)Math.Clamp(c.G + g, 0, 255), 
                (byte)Math.Clamp(c.B + b, 0, 255), 
                c.A);
        }
    }
}
