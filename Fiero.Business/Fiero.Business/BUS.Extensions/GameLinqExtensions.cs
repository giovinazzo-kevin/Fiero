using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{

    public static class GameLinqExtensions
    {
        public static IEnumerable<Actor> Players(this IEnumerable<Entity> entities)
            => entities.OfType<Actor>().Where(a => a.IsPlayer());
    }
}
