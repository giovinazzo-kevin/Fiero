using Fiero.Core;

namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ExplosionHappenedEvent(Entity Source, Coord Center, Coord[] Points, int BaseDamage);
    }
}
