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
    }
}
