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
        public static Random Random => _rng.Value;
        public static Random Seeded(int seed) => new(seed);
    }
}
