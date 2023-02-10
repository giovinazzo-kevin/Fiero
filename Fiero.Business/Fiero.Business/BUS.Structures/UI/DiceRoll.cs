using Fiero.Core;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public readonly record struct Dice(int NumberOfDice, int NumberOfSides)
    {
        public IEnumerable<int> Roll(Random rng)
        {
            for (int i = 0; i < NumberOfDice; i++)
            {
                yield return rng.Next(NumberOfSides);
            }
        }
        public IEnumerable<int> Roll() => Roll(Rng.Random);

        public static IEnumerable<int> Roll(Random rng, int numDice, int numSides) => new Dice(numDice, numSides).Roll(rng);
        public static IEnumerable<int> Roll(int numDice, int numSides) => new Dice(numDice, numSides).Roll();

        public void Do(Random rng, Action<int> @do)
        {
            foreach (var i in Roll(rng)) @do(i);
        }

        public void Do(Action<int> @do) => Do(Rng.Random, @do);
        public static void Do(Random rng, int numDice, int numSides, Action<int> @do) => new Dice(numDice, numSides).Do(rng, @do);
        public static void Do(int numDice, int numSides, Action<int> @do) => new Dice(numDice, numSides).Do(@do);
    }
}
