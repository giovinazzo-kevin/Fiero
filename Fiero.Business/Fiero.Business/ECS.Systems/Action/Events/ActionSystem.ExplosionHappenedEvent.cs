using Fiero.Core;

namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ExplosionHappenedEvent
        {
            public readonly Entity Source;
            public readonly Coord Center;
            public readonly Coord[] Points;
            public readonly int BaseDamage;
            public ExplosionHappenedEvent(Entity source, Coord center, Coord[] points, int dmg)
                => (Source, Center, Points, BaseDamage) = (source, center, points, dmg);
        }
    }
}
