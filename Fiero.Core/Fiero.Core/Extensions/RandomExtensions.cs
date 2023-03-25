using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Core.Extensions
{
    public static class RandomExtensions
    {
        public static T Choose<T>(this Random rng, IList<T> source) => source.Shuffle(rng).First();
        public static T ChooseWeighted<T>(this Random rng, params (T Item, float Weight)[] source)
        {
            var dist = rng.NextDouble() * source.Sum(s => s.Weight);
            for (int i = 0; i < source.Length; i++)
            {
                dist -= source[i].Weight;
                if (dist < 0)
                    return source[i].Item;
            }
            throw new InvalidOperationException();
        }
        public static bool NChancesIn(this Random rng, float numerator, float denominator)
        {
            if (numerator == 0 || denominator == 0)
                return false;
            if (numerator == denominator)
                return true;
            return rng.NextDouble() < numerator / denominator;
        }
        public static int Between(this Random rng, int min, int max)
            => rng.Next(min, max + 1);
        public static double Between(this Random rng, double min, double max)
            => min + rng.NextDouble() * (max - min);

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, Random rng)
        {
            T[] elements = source.ToArray();
            // Note i > 0 to avoid final pointless iteration
            for (int i = elements.Length - 1; i > 0; i--)
            {
                // Swap element "i" with a random earlier element it (or itself)
                int swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
                // we don't actually perform the swap, we can forget about the
                // swapped element because we already returned it.
            }
            if (elements.Length == 0)
                yield break;
            // there is one item remaining that was not returned - we return it now
            yield return elements[0];
        }
    }
}
