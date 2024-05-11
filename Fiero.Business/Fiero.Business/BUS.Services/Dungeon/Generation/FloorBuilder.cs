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
                .Where(x => x.Build != null)
                .Select(o => o.Build(id))
                .ToList();
            // Place all tiles that were set in the context, including objects that eventually resolve to tiles
            var tileObjects = objects.TrySelect(e => (e.TryCast<Tile>(out var t), t))
                .ToList();
            foreach (var tile in tileObjects)
            {
                floor.SetTile(tile);
            }
            var variantsToCheck = new HashSet<Tile>();
            foreach (var tileDef in context.GetTiles())
            {
                if (tileObjects.Any(t => t.Position() == tileDef.Position))
                    continue;
                if (tileDef.Name != TileName.None)
                {
                    var tile = tileDef.Resolve(_entityBuilders, id).Build();
                    floor.SetTile(tile);
                    if (tile.TileProperties.Variants.Any())
                        variantsToCheck.Add(tile);
                }
            }
            // Apply all tile variants
            foreach (var check in variantsToCheck
                .OrderBy(x => x.TileProperties.Variants.Max(y => y.Precedence)))
            {
                var cells = floor.GetNeighborhood(check.Position(), size: 3, yieldNull: true)
                    .Select(x => x?.Tile.TileProperties.Name ?? TileName.None)
                    .ToArray();
                var matrix = new TileNeighborhood(
                    cells[0], cells[1], cells[2],
                    cells[3], cells[4], cells[5],
                    cells[6], cells[7], cells[8]
                );
                foreach (var rule in check.TileProperties.Variants)
                {
                    if (!rule.Matrix.Matches(matrix))
                        continue;
                    check.Render.Sprite = rule.Variant;
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
            // Set spawn points for the player
            floor.SpawnPoints.AddRange(context.GetSpawnPoints());
            return floor;
        }
    }
}
