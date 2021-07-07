using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public static class GameLinqExtensions
    {
        public static IEnumerable<U> TrySelect<T, U>(this IEnumerable<T> source, Func<T, (bool, U)> tryGet)
        {
            return source.Select(tryGet)
                .Where(x => x.Item1)
                .Select(x => x.Item2);
        }

        public static IEnumerable<Actor> Players(this IEnumerable<Entity> entities)
            => entities.OfType<Actor>().Where(a => a.ActorProperties.Type == ActorName.Player);
    }
}
