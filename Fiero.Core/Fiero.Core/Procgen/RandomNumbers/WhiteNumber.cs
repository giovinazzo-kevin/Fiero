using System;

namespace Fiero.Core
{
    public class WhiteNumber : IRandomNumber
    {
        private readonly Random _rng;
        public int Range { get; }

        public WhiteNumber(int range = 128)
        {
            _rng = new Random();
            Range = range;
        }

        public int Next()
        {
            return _rng.Next() % Range;
        }
    }
}
