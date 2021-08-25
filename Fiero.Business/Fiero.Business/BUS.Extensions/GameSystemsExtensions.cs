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
        public static bool TryPlace(this GameSystems systems, FloorId floorId, Item item, float maxDistance = 10)
        {
            if (!systems.Floor.TryGetClosestFreeTile(floorId, item.Position(), out var spawnTile, maxDistance)) {
                return false;
            }
            item.Physics.Position = spawnTile.Position();
            systems.Floor.AddItem(floorId, item);
            return true;
        }
    }
}
