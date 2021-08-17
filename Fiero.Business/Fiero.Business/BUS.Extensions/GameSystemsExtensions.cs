namespace Fiero.Business
{
    public static class GameSystemsExtensions
    {
        public static bool TrySpawn(this GameSystems systems, FloorId floorId, Actor actor, float maxDistance = 10)
        {
            if (!systems.Floor.TryGetClosestFreeTile(floorId, actor.Position(), out var spawnTile, maxDistance)) {
                return false;
            }
            actor.Physics.Position = spawnTile.Position();
            systems.Action.Track(actor.Id);
            systems.Action.Spawn(actor);
            systems.Floor.AddActor(floorId, actor);
            return true;
        }
    }
}
