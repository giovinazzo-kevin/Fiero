namespace Fiero.Business
{

    [TransientDependency]
    public sealed class FloorBuilder
    {
        private readonly GameEntityBuilders _entityBuilders;
        private readonly List<Action<FloorGenerationContext>> _steps;

        public FloorBuilder(GameEntityBuilders builders)
        {
            _entityBuilders = builders;
            _steps = new List<Action<FloorGenerationContext>>();
        }

        public FloorBuilder WithStep(Action<FloorGenerationContext> step)
        {
            _steps.Add(step);
            return this;
        }

        public Floor Build(FloorId id, Coord size)
        {
            var floor = new Floor(id, size);
            var context = new FloorGenerationContext(_entityBuilders, id, size);
            // Run user steps, initializing the context
            foreach (var step in _steps)
            {
                step(context);
            }
            // Get all objects that were added to the context, but exclude portals and stairs which need special handling
            var objects = context.GetObjects()
                .Select(o => o.Build(id))
                .ToList();
            // Place all tiles that were set in the context, including objects that eventually resolve to tiles
            var tileObjects = objects.TrySelect(e => (e.TryCast<Tile>(out var t), t))
                .ToList();
            foreach (var tile in tileObjects)
            {
                floor.SetTile(tile);
            }
            foreach (var tileDef in context.GetTiles())
            {
                if (tileObjects.Any(t => t.Position() == tileDef.Position))
                    continue;
                if (tileDef.Name != TileName.None)
                {
                    floor.SetTile(tileDef.Resolve(_entityBuilders, id).Build());
                }
            }
            // Place all features that were added to the context
            var featureObjects = objects.TrySelect(e => (e.TryCast<Feature>(out var f), f))
                .ToList();
            foreach (var feature in featureObjects)
            {
                floor.AddFeature(feature);
            }
            // Place all items that were added to the context
            var itemObjects = objects.TrySelect(e => (e.TryCast<Item>(out var i), i));
            foreach (var item in itemObjects)
            {
                floor.AddItem(item);
            }
            // Spawn all enemies and actors that were added to the context
            var actorObjects = objects.TrySelect(e => (e.TryCast<Actor>(out var a), a));
            foreach (var actor in actorObjects)
            {
                floor.AddActor(actor);
            }
            return floor;
        }
    }
}
