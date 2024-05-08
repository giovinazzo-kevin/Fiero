namespace Fiero.Business
{
    public abstract class RoomTreeGenerator : IBranchGenerator
    {
        public readonly record struct EnemyPoolArgs(Room CurrentRoom, FloorGenerationContext Context, GameEntityBuilders Entities);
        public readonly record struct ItemPoolArgs(Room CurrentRoom, FloorGenerationContext Context, GameEntityBuilders Entities);

        public abstract Coord MapSize(FloorId id);
        public abstract Coord GridSize(FloorId id);
        public readonly DungeonTheme Theme;

        public RoomTreeGenerator(DungeonTheme theme)
        {
            Theme = theme;
        }

        protected virtual void MarkConnectors(List<RoomSector> roomSectors, List<Corridor> corridors)
        {
            // MarkSharedConnectors will flag all room connectors that are
            // directly adjacent to connectors from separate rooms of other sectors.
            // This will make them "invisible" and merge the rooms into one large area.
            RoomSector.MarkSharedConnectors(roomSectors);
            // MarkActiveConnectors will flag all room connectors that are being
            // used either as part of an intra- or of an inter-sector corridor.
            // This lets the room know which points should remain connected.
            foreach (var sector in roomSectors)
                sector.MarkActiveConnectors(corridors);
            // Secret corridors have fake doors that look just like walls!
            // A scroll of magic mapping, and some skills, will reveal them.
            foreach (var n in Theme.SecretCorridors.Roll())
            {
                var sector = Rng.Random.Choose(roomSectors);
                sector.MarkSecretCorridors(n);
            }
        }

        protected virtual RoomTree BuildTree(FloorId id, Coord mapSize, Coord gridSize)
        {
            var roomPool = ConfigureRoomPool(id, new()).Build(capacity: gridSize == Coord.Zero ? 1 : gridSize.Area() * 8);
            var roomSectors = RoomSector.CreateTiling(mapSize, gridSize, Theme.RoomSquares, roomPool)
                .ToList();
            var corridors = RoomSector.GenerateInterSectorCorridors(roomSectors)
                .ToList();
            MarkConnectors(roomSectors, corridors);
            var tree = RoomTree.Build(
                roomSectors.SelectMany(s => s.Rooms).ToArray(),
                corridors.Concat(roomSectors.SelectMany(s => s.Corridors)).ToArray()
            );
            tree.SetTheme(Theme, ShouldApplyTheme);
            var totalRects = roomSectors.SelectMany(x => x.Rooms).Select(x => x.GetRects().Count()).Sum();
            var enemyPool = ConfigureEnemyPool(id, new()).Build(capacity: totalRects * 4);
            var itemPool = ConfigureItemPool(id, new()).Build(capacity: totalRects * 4);
            foreach (var (par, cor, chd) in tree.Traverse())
            {
                if (chd == null)
                    continue;
                chd.Room.Drawn += OnRoomDrawn_;
            }

            return tree;

            void OnRoomDrawn_(Room room, FloorGenerationContext ctx)
            {
                OnRoomDrawn(room, ctx, enemyPool, itemPool);
            }
        }

        protected virtual bool ShouldApplyTheme(IFloorGenerationPrefab prefab) =>
            prefab is EmptyRoom
            || prefab is Corridor;


        protected abstract PoolBuilder<Func<EnemyPoolArgs, IEntityBuilder<Actor>>> ConfigureEnemyPool(FloorId id, PoolBuilder<Func<EnemyPoolArgs, IEntityBuilder<Actor>>> pool);
        protected abstract PoolBuilder<Func<Room>> ConfigureRoomPool(FloorId id, PoolBuilder<Func<Room>> pool);
        protected abstract PoolBuilder<Func<ItemPoolArgs, IEntityBuilder<Item>>> ConfigureItemPool(FloorId id, PoolBuilder<Func<ItemPoolArgs, IEntityBuilder<Item>>> pool);
        protected virtual Dice GetMonsterDice(Room room, FloorGenerationContext ctx) => new(2, room.GetRects().Count());
        protected virtual Dice GetItemDice(Room room, FloorGenerationContext ctx) => new(3, 2, Bias: -1);
        protected virtual Dice GetTrapDice(Room room, FloorGenerationContext ctx) => new(1, 2);

        protected virtual void OnRoomDrawn(Room room, FloorGenerationContext ctx, Pool<Func<EnemyPoolArgs, IEntityBuilder<Actor>>> enemyPool, Pool<Func<ItemPoolArgs, IEntityBuilder<Item>>> itemPool)
        {
            var candidateTiles = room.GetPointCloud()
                .Where(p => ctx.GetTile(p).Name == TileName.Room)
                .ToList();
            if (room.AllowMonsters)
            {
                var numMonsters = GetMonsterDice(room, ctx)
                    .Roll(Rng.Random).Sum();
                for (int i = 0; i < numMonsters && candidateTiles.Any(); i++)
                {
                    var p = Rng.Random.Choose(candidateTiles);
                    candidateTiles.Remove(p);
                    ctx.AddObject("monster", p, entities => enemyPool.Next()(new(room, ctx, entities)));
                }
            }
            if (room.AllowItems)
            {
                var numItems = GetItemDice(room, ctx)
                    .Roll(Rng.Random).Sum();
                for (int i = 0; i < numItems && candidateTiles.Any(); i++)
                {
                    var p = Rng.Random.Choose(candidateTiles);
                    candidateTiles.Remove(p);
                    ctx.AddObject("item", p, entities => itemPool.Next()(new(room, ctx, entities)));
                }
            }
            if (room.AllowTraps)
            {
                var numTraps = GetTrapDice(room, ctx)
                    .Roll(Rng.Random).Sum();
                for (int i = 0; i < numTraps && candidateTiles.Any(); i++)
                {
                    var p = Rng.Random.Choose(candidateTiles);
                    candidateTiles.Remove(p);
                    ctx.TryAddFeature("trap", p, entities => entities.Feature_Trap());
                }
            }
        }

        protected virtual bool AllowStairsOn(TileName name) => name switch
        {
            TileName.Room => true,
            _ => false
        };

        protected virtual void PlaceStairs(FloorGenerationContext ctx, RoomTree tree, FloorId floorId)
        {
            var centralNode = tree.Nodes
                .MaxBy(x => x.Centrality);
            Coord knownStairs = default;
            foreach (var conn in ctx.GetConnections().OrderBy(c => c.From.Branch).ThenBy(c => c.From.Depth))
            {
                // Add upstairs and downstairs to respective floors
                var emptyTiles = ctx.GetEmptyTiles()
                    .Where(t => AllowStairsOn(t.Name))
                    .Select(x => x.Position)
                    .Shuffle(Rng.Random);
                if (!emptyTiles.Any())
                    throw new InvalidOperationException("No empty tiles on which to place stairs");
                if (conn.From == floorId)
                {
                    var downstairsCandidates = emptyTiles
                        .OrderByDescending(x => x.DistSq(knownStairs));
                    ctx.TryAddFeature("Downstairs", downstairsCandidates, e => e.Feature_Downstairs(conn), out knownStairs);
                }
                else
                {
                    // Place the upstairs in the most central node of the graph. The downstairs will be placed in the furthest leaf node.
                    var upstairsCandidates = emptyTiles
                        .Where(t => centralNode.Room.GetRects().Any(r => r.Contains(t.X, t.Y)))
                        .OrderByDescending(x => x.DistSq(knownStairs));
                    ctx.TryAddFeature("Upstairs", upstairsCandidates, e => e.Feature_Upstairs(conn), out knownStairs);
                }
            }
        }

        protected virtual void ApplyThemeRules(FloorGenerationContext ctx)
        {
            foreach (var rule in Theme.Rules)
                ctx.Rule(rule);
        }

        public Floor GenerateFloor(FloorId id, FloorBuilder builder)
        {
            var mapSize = MapSize(id);
            var gridSize = GridSize(id);
            var tree = BuildTree(id, mapSize, gridSize);
            return builder
                .WithStep(tree.Draw)
                .WithStep(ctx => PlaceStairs(ctx, tree, id))
                .WithStep(ApplyThemeRules)
                .Build(id, mapSize);
        }
    }
}
