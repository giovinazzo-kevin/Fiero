using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Core
{
    public static class RandomExtensions
    {
        public static T Choose<T>(this Random rng, params T[] source)
        {
            return source[rng.Next(source.Length)];
        }
        public static bool OneChanceIn(this Random rng, int denominator)
        {
            return rng.NextDouble() < 1f / denominator;
        }
    }
}
