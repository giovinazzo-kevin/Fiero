using Fiero.Core.Structures;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Core.Extensions
{
    public static class GameLinqExtensions
    {
        public static IEnumerable<T> OfAssignableType<_, T>(this IEnumerable<_> source)
        {
            return source.Where(x => x.GetType().IsAssignableTo(typeof(T))).Cast<T>();
        }

        public static IEnumerable<UnorderedPair<A>> Pairs<A>(this IEnumerable<A> source)
        {
            return source.SelectMany(a =>
                source.Where(b => !Equals(b, a))
                .Select(b => new UnorderedPair<A>(a, b)))
                .DistinctBy(x => x.GetHashCode());
        }

        public static IEnumerable<UnorderedPair<A>> Pairs<A>(this IEnumerable<A> source, IEnumerable<A> other)
        {
            return source.SelectMany(a =>
                other.Where(b => !Equals(b, a))
                .Select(b => new UnorderedPair<A>(a, b)))
                .DistinctBy(x => x.GetHashCode());
        }
    }
}
