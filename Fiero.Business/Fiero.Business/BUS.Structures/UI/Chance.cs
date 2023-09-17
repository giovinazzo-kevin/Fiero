using Fiero.Core;

using System;

namespace Fiero.Business
{
    public readonly record struct Chance(int Numerator, int Denominator)
    {
        public static readonly Chance Always = new(1, 1);
        public static readonly Chance FiftyFifty = new(1, 2);
        public static readonly Chance Never = new(0, 0);

        public bool Check(Random rng) => rng.NChancesIn(Numerator, Denominator);
        public bool Check() => Check(Rng.Random);
        public static bool Check(Random rng, int numerator, int denominator) => new Chance(numerator, denominator).Check(rng);
        public static bool Check(int numerator, int denominator) => new Chance(numerator, denominator).Check();
        public static bool OneIn(Random rng, int denominator) => new Chance(1, denominator).Check(rng);
        public static bool OneIn(int denominator) => new Chance(1, denominator).Check();
    }
}
