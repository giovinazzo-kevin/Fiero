namespace Fiero.Business
{
    public readonly record struct Dice(int NumberOfDice, int NumberOfSides, Func<int, int, double> Weights = null, int Bias = 0)
    {
        public Dice Unweighted() => new(NumberOfDice, NumberOfSides);
        public Dice Weighted(Func<int, int, double> Weights) => new(NumberOfDice, NumberOfSides, Weights);

        private IEnumerable<int> RollFair(Random rng)
        {
            if (NumberOfDice <= 0 || NumberOfSides <= 0)
            {
                yield return 0;
                yield break;
            }
            for (int i = 0; i < NumberOfDice; i++)
            {
                yield return rng.Next(1, NumberOfSides + 1) + Bias;
            }
        }
        private IEnumerable<int> RollWeighted(Random rng)
        {
            if (NumberOfDice <= 0 || NumberOfSides <= 0)
            {
                yield return 0;
                yield break;
            }

            var weightFunction = Weights;
            var memoizedWeights = new Dictionary<(int, int), double>();
            double maxWeight = double.MinValue;
            double memoizedWeightFunction(int dieIndex, int faceIndex)
            {
                if (!memoizedWeights.TryGetValue((dieIndex, faceIndex), out double weight))
                {
                    weight = weightFunction(dieIndex, faceIndex);
                    memoizedWeights[(dieIndex, faceIndex)] = weight;
                    maxWeight = Math.Max(maxWeight, weight);
                }
                return weight;
            }

            // Calculate maxWeight and populate memoizedWeights
            for (int dieIndex = 0; dieIndex < NumberOfDice; dieIndex++)
                for (int faceIndex = 1; faceIndex <= NumberOfSides; faceIndex++)
                    memoizedWeightFunction(dieIndex, faceIndex);

            // Now, adjust all weights to be between 0 and 1
            foreach (var key in memoizedWeights.Keys.ToList())
                memoizedWeights[key] /= maxWeight;

            for (int i = 0; i < NumberOfDice; i++)
            {
                while (true)
                {
                    int roll = rng.Next(1, NumberOfSides + 1) + Bias;
                    if (rng.NextDouble() <= memoizedWeightFunction(i, roll))
                    {
                        yield return roll;
                        break;
                    }
                }
            }
        }

        public IEnumerable<int> Roll(Random rng) => Weights != null ? RollWeighted(rng) : RollFair(rng);
        public IEnumerable<int> Roll() => Weights != null ? RollWeighted(Rng.Random) : RollFair(Rng.Random);
        public static IEnumerable<int> Roll(Random rng, int numDice, int numSides, Func<int, int, double> weights = null)
            => new Dice(numDice, numSides, weights).Roll(rng);
        public static IEnumerable<int> Roll(int numDice, int numSides, Func<int, int, double> weights = null)
            => new Dice(numDice, numSides, weights).Roll();
        public void Do(Random rng, Action<int> @do)
        {
            foreach (var i in Roll(rng)) @do(i);
        }

        public void Do(Action<int> @do) => Do(Rng.Random, @do);
        public static void Do(Random rng, int numDice, int numSides, Action<int> @do) => new Dice(numDice, numSides).Do(rng, @do);
        public static void Do(int numDice, int numSides, Action<int> @do) => new Dice(numDice, numSides).Do(@do);
    }
}
