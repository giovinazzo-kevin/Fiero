using Simplex;

namespace Fiero.Core
{
    public abstract class SimplexNumber : IRandomNumber
    {
        protected readonly Noise Noise;
        public int Range { get; }
        public float Scale { get; }

        public SimplexNumber(float scale, int range = 128, Noise rng = null)
        {
            Noise = rng ?? new Noise();
            Range = range;
            Scale = scale;
        }

        public abstract int Next();
    }

    public class SimplexNumber1D : SimplexNumber
    {
        public int X { get; }
        public SimplexNumber1D(int x, float scale = 1f, int range = 128, Noise rng = null)
            : base(scale, range, rng) { X = x; }
        public override int Next() => (int)(Noise.CalcPixel1D(X, Scale) / 256f * Range);
    }

    public class SimplexNumber2D : SimplexNumber
    {
        public int X { get; }
        public int Y { get; }
        public SimplexNumber2D(int x, int y, float scale = 1f, int range = 128, Noise rng = null)
            : base(scale, range, rng) { X = x; Y = y; }
        public override int Next() => (int)(Noise.CalcPixel2D(X, Y, Scale) / 256f * Range);
    }

    public class SimplexNumber3D : SimplexNumber
    {
        public int X { get; }
        public int Y { get; }
        public int Z { get; }
        public SimplexNumber3D(int x, int y, int z, float scale = 1f, int range = 128, Noise rng = null)
            : base(scale, range, rng) { X = x; Y = y; Z = z; }
        public override int Next() => (int)(Noise.CalcPixel3D(X, Y, Z, Scale) / 256f * Range);
    }
}
