using System;

namespace Fiero.Core
{
    public class PinkNumber : IRandomNumber
    {
        private readonly Random _rng;
        private readonly int[] _whiteValues;

        private int _key;
        private int _maxKey;

        public int Range { get; }

        public PinkNumber(int range = 128, Random rng = null)
        {
            _maxKey = 0x1f; // Five bits set
            Range = range;
            _key = 0;
            _rng = rng ?? new Random();
            _whiteValues = new int[5];
            for (int i = 0; i < _whiteValues.Length; i++)
            {
                _whiteValues[i] = _rng.Next() % (range / 5);
            }
        }

        public int Next()
        {
            int sum;
            var last_key = _key;

            _key++;
            if (_key > _maxKey)
                _key = 0;
            // Exclusive-Or previous value with current value. This gives
            // a list of bits that have changed.
            int diff = last_key ^ _key;
            sum = 0;
            for (int i = 0; i < 5; i++)
            {
                // If bit changed get new random number for corresponding
                // white_value
                if ((diff & (1 << i)) != 0)
                    _whiteValues[i] = _rng.Next() % (Range / 5);
                sum += _whiteValues[i];
            }
            return sum;
        }
    };
}
