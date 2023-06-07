using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Core.Extensions
{
    public static class RandomExtensions
    {
        public static T Choose<T>(this Random rng, IList<T> source) => source.Shuffle(rng).First();
        public static T ChooseWeighted<T>(this Random rng, params WeightedItem<T>[] source) => ChooseWeighted(rng, (IList<WeightedItem<T>>)source);
        public static T ChooseWeighted<T>(this Random rng, IList<WeightedItem<T>> source)
        {
            var dist = rng.NextDouble() * source.Sum(s => s.Weight);
            for (int i = 0; i < source.Count; i++)
            {
                dist -= source[i].Weight;
                if (dist < 0)
                    return source[i].Item;
            }
            throw new InvalidOperationException();
        }
        public static List<T> ChooseMultipleGuaranteedWeighted<T>(this Random rng, IList<GuaranteedWeightedItem<T>> source, int numSelections)
        {
            var sum = source.Sum(s => s.MinAmount);
            if (numSelections < sum)
                throw new ArgumentException($"Sum of minimum amounts ({sum}) exceeds total number of selections ({numSelections}).");

            var results = new List<T>();
            var remainingItems = new List<WeightedItem<T>>(source);

            // Prepare list of guaranteed items
            var guaranteedItems = new List<T>();
            foreach (var item in source)
            {
                for (int i = 0; i < item.MinAmount; i++)
                {
                    guaranteedItems.Add(item.Item);
                }
            }

            // Interleave guaranteed items with random selections
            while (results.Count < numSelections)
            {
                if (rng.NextDouble() < (double)guaranteedItems.Count / (numSelections - results.Count) && guaranteedItems.Count > 0)
                {
                    // Select a random guaranteed item
                    int index = rng.Next(guaranteedItems.Count);
                    T chosenItem = guaranteedItems[index];
                    guaranteedItems.RemoveAt(index);

                    results.Add(chosenItem);
                }
                else
                {
                    // Select a weighted random remaining item
                    var chosenItem = rng.ChooseWeighted(remainingItems);

                    results.Add(chosenItem);
                }
            }

            return results;
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
        /// <summary>
        /// Rounds a number with a 50% chance of either rounding down or up.
        /// </summary>
        public static int Round(this Random rng, double number) =>
            rng.NChancesIn(1, 2) ? (int)Math.Floor(number) : (int)Math.Ceiling(number);
        /// <summary>
        /// Rounds a number with a chance of rounding down or up that depends on how close that number is to 0.5, where it is 50%.
        /// </summary>
        public static int RoundProportional(this Random rng, double number)
        {
            var decimalPart = number - Math.Floor(number);
            // If the generated random number is less than the decimal part, we round up, otherwise round down.
            return rng.NextDouble() < decimalPart ? (int)Math.Ceiling(number) : (int)Math.Floor(number);
        }

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
