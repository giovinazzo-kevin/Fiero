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
    }
}
