using SFML.Graphics;
using System;

namespace Fiero.Core
{
    public class TextureGenerator
    {
        public static Image WhiteNoise(int w, int h)
        {
            using var grid = new HardwareGrid(w, h);
            grid.SetPixels(xy => {
                var rng = new Random();
                var f = (byte)rng.Next(256);
                return new(f, f, f, 255);
            });
            return grid.CopyToImage();
        }

        public static Image PinkNoise(int w, int h)
        {
            using var grid = new HardwareGrid(w, h);
            grid.SetPixels(xy => {
                var pink = new PinkNumber(256);
                var f = (byte)pink.Next();
                return new(f, f, f, 255);
            });
            return grid.CopyToImage();
        }
    }
}
