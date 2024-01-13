namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ExplosionHappenedEvent(Entity Cause, Entity Source, FloorId FloorId, Coord Center, Coord[] Points, int BaseDamage);
    }
}
