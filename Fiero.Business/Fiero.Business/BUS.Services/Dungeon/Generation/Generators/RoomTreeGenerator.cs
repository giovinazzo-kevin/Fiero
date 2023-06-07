using Fiero.Core;
using Fiero.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public abstract class RoomTreeGenerator : IBranchGenerator
    {
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

        protected virtual RoomTree BuildTree(Coord mapSize, Coord gridSize)
        {
            var pool = ConfigureRoomPool(new PoolBuilder<Func<Room>>()).Build(capacity: gridSize.Area() * 8);
            var roomSectors = RoomSector.CreateTiling(mapSize, gridSize, Theme.RoomSquares, pool)
                .ToList();
            var corridors = RoomSector.GenerateInterSectorCorridors(roomSectors)
                .ToList();
            MarkConnectors(roomSectors, corridors);
            var tree = RoomTree.Build(
                roomSectors.SelectMany(s => s.Rooms).ToArray(),
                corridors.Concat(roomSectors.SelectMany(s => s.Corridors)).ToArray()
            );
            tree.SetTheme(Theme, ShouldApplyTheme);
            foreach (var (par, cor, chd) in tree.Traverse())
            {
                if (par != null)
                    par.Room.Drawn += OnRoomDrawn;
            }
            return tree;
        }

        protected virtual bool ShouldApplyTheme(IFloorGenerationPrefab prefab) =>
            prefab is EmptyRoom
            || prefab is Corridor;

        protected abstract PoolBuilder<Func<Room>> ConfigureRoomPool(PoolBuilder<Func<Room>> pool);

        protected abstract void OnRoomDrawn(Room room, FloorGenerationContext ctx);

        protected virtual void PlaceStairs(FloorGenerationContext ctx, RoomTree tree, FloorId floorId)
        {
            var centralNode = tree.Nodes
                .MaxBy(x => x.Centrality);
            foreach (var conn in ctx.GetConnections())
            {
                // Add upstairs and downstairs to respective floors
                var emptyTiles = ctx.GetEmptyTiles()
                    .Where(t => t.Name == TileName.Room && centralNode.Room.GetRects().Any(r => r.Contains(t.Position.X, t.Position.Y)))
                    .Select(x => x.Position)
                    .Shuffle(Rng.Random);
                if (!emptyTiles.Any())
                    throw new InvalidOperationException("No empty tiles on which to place stairs");
                if (conn.From == floorId)
                {
                    ctx.TryAddFeature("Downstairs", emptyTiles, e => e.Feature_Downstairs(conn), out _);
                }
                else
                {
                    ctx.TryAddFeature("Upstairs", emptyTiles, e => e.Feature_Upstairs(conn), out _);
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
            var tree = BuildTree(mapSize, gridSize);
            return builder
                .WithStep(tree.Draw)
                .WithStep(ctx => PlaceStairs(ctx, tree, id))
                .WithStep(ApplyThemeRules)
                .Build(id, mapSize);
        }
    }
}
