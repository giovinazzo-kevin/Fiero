using System;

namespace Fiero.Core
{
    public class GaussianNumber : IRandomNumber
    {
        private readonly Random _rng;
        public int Range { get; }
        public double Mean { get; }
        public double StandardDeviation { get; }

        public GaussianNumber(double mean, double stdDev, int range = 128)
        {
            _rng = new Random();
            Mean = mean;
            StandardDeviation = stdDev;
            Range = range;
        }

        public int Next()
        {
            // Uniform(0,1] random doubles
            var u1 = 1.0 - _rng.NextDouble();
            var u2 = 1.0 - _rng.NextDouble();
            // Random normal(0,1)
            var randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
            // Random normal(mean,stdDev^2)
            var randNormal = Mean + StandardDeviation * randStdNormal; 
            return (int)(randNormal * Range);
        }
    }
}
