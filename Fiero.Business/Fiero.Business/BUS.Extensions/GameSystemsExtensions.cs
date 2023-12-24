namespace Fiero.Business
{
    public static class MetaSystemExtensions
    {
        public static bool TrySpawn(this MetaSystem systems, FloorId floorId, Actor actor, float maxDistance = 24)
        {
            if (!systems.Get<DungeonSystem>().TryGetClosestFreeTile(floorId, actor.Position(), out var spawnTile, maxDistance,
                c => !c.Actors.Any()))
            {
                return false;
            }
            actor.Physics.Position = spawnTile.Tile.Position();
            systems.Get<ActionSystem>().Track(actor.Id);
            systems.Get<ActionSystem>().Spawn(actor);
            systems.Get<DungeonSystem>().AddActor(floorId, actor);
            return true;
        }
        public static bool TryPlace(this MetaSystem systems, FloorId floorId, Item item, float maxDistance = 24)
        {
            if (!systems.Get<DungeonSystem>().TryGetClosestFreeTile(floorId, item.Position(), out var spawnTile, maxDistance,
                c => !c.Items.Any()))
            {
                return false;
            }
            item.Physics.Position = spawnTile.Tile.Position();
            systems.Get<DungeonSystem>().AddItem(floorId, item);
            return true;
        }
    }
}
