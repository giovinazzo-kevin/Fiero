using Simplex;
using System;
using System.Threading;

namespace Fiero.Core
{
    public static class Rng
    {
        private static int _staticSeed = Environment.TickCount;
        private static readonly ThreadLocal<Random> _rng = new(() =>
        {
            var seed = Interlocked.Increment(ref _staticSeed) & 0x7FFFFFFF;
            return new Random(seed);
        });
        private static readonly ThreadLocal<Noise> _noise = new(() =>
        {
            var seed = Interlocked.Increment(ref _staticSeed) & 0x7FFFFFFF;
            return new Noise { Seed = seed };
        });
        public static Random Random => _rng.Value;
        public static Noise Noise => _noise.Value;
        public static Random SeededRandom(int seed) => new(seed);
        public static Noise SeededNoise(int seed) => new Noise { Seed = seed };
        public static void SetGlobalSeed(int seed)
        {
            _staticSeed = seed;
            _rng.Value = new Random(seed);
        }
        public static int GetGlobalSeed() => _staticSeed;
    }
}
