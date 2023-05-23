using SFML.Graphics;
using Simplex;
using System;

namespace Fiero.Core
{
    public class TextureGenerator
    {
        public static Image Noise<T>(int w, int h, Func<Coord, T> gen)
            where T : IRandomNumber
        {
            using var grid = new HardwareGrid(w, h);
            grid.SetPixels(xy =>
            {
                var number = gen(xy);
                var f = (byte)((number.Next() / (double)number.Range) * 255);
                return new(f, f, f, 255);
            });
            return grid.CopyToImage();
        }

        public static Image WhiteNoise(int w, int h, Random rng = null)
        {
            var number = new WhiteNumber(range: 256, rng: rng);
            return Noise(w, h, _ => number);
        }
        public static Image PinkNoise(int w, int h, Random rng = null)
        {
            var number = new PinkNumber(range: 256, rng: rng);
            return Noise(w, h, _ => number);
        }
        public static Image GaussianNoise(int w, int h, double mean, double stdDev, Random rng = null)
        {
            var number = new GaussianNumber(mean, stdDev, range: 256, rng: rng);
            return Noise(w, h, _ => number);
        }
        public static Image SimplexNoise(int w, int h, float scale, Noise rng = null)
        {
            return Noise(w, h, xy => new SimplexNumber2D(xy.X, xy.Y, scale, range: 256, rng));
        }
    }
}
