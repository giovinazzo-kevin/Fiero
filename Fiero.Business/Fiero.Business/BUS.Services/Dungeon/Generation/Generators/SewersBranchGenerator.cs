using Fiero.Core;
using Fiero.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Fiero.Business
{
    public class SewersBranchGenerator : BranchGenerator
    {
        public static readonly DungeonTheme Theme = DungeonTheme.Default with
        {
            WallTile = (c => DungeonTheme.Default.WallTile(c).WithCustomColor(ColorName.LightGreen)),
            RoomTile = (c => DungeonTheme.Default.RoomTile(c).WithCustomColor(ColorName.Gray)),
            CorridorTile = (c => DungeonTheme.Default.CorridorTile(c).WithCustomColor(ColorName.Green)),
            MaxRoomSquares = new(1, 6, (d, s) => 1f / Math.Pow(s, 2))
        };


        public override Floor GenerateFloor(FloorId floorId, FloorBuilder builder)
        {
            var (size, subdivisions) = floorId.Depth switch
            {
                _ => (new Coord(50, 50), new Coord(2, 2)),
            };
            var roomSectors = RoomSector.CreateTiling(size, subdivisions, Theme.MaxRoomSquares, CreateRoom)
                .ToList();
            var corridors = RoomSector.GenerateInterSectorCorridors(roomSectors)
                .ToList();

            // MarkActiveConnectors will flag all room connectors that are being
            // used either as part of an intra- or of an inter-sector corridor.
            // This lets the room know which points should remain connected.
            foreach (var sector in roomSectors)
                sector.MarkActiveConnectors(corridors);
            RoomSector.MarkSharedConnectors(roomSectors);

            var tree = RoomTree.Build(
                roomSectors.SelectMany(s => s.Rooms).ToArray(),
                corridors.Concat(roomSectors.SelectMany(s => s.Corridors)).ToArray()
            );
            tree.SetTheme(Theme,
                prefab => prefab is EmptyRoom
                       || prefab is Corridor);

            var roomCount = roomSectors.SelectMany(x => x.Rooms).Count();
            var numSecrets = new Dice(2, 2);
            // Secret corridors have fake doors that look just like walls!
            // A scroll of magic mapping, and some skills, will reveal them.
            foreach (var n in numSecrets.Roll())
            {
                var sector = Rng.Random.Choose(roomSectors);
                sector.MarkSecretCorridors(n);
            }

            var centralNode = tree.Nodes
                .MaxBy(x => x.Centrality);

            return builder
                .WithStep(tree.Draw)
                .WithStep(ctx =>
                {
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
                })
                .Build(floorId, size);

            Room CreateRoom(ImmutableArray<RoomRect> rects)
            {
                Room room = Rng.Random.ChooseWeighted(new (Func<Room>, float)[] {
                    (() => new ShrineRoom(),    0.5f / rects.Length),
                    (() => new TreasureRoom(),  1.0f / rects.Length),
                    (() => new WetFloorSewerRoom() ,  44.25f * rects.Length),
                    (() => new EmptyRoom() ,    44.25f * rects.Length)
                })();
                foreach (var rect in rects)
                    room.AddRect(rect);
                room.Drawn += (r, ctx) =>
                {
                    var area = r.GetRects().Count(); // Chances are not actually per-room but per room square
                    var pointCloud = new Queue<Coord>(r.GetPointCloud().Shuffle(Rng.Random));
                    if (r.AllowMonsters)
                    {
                        var roll = new Dice(3, 7);
                        roll.Do(i =>
                        {
                            if (Chance.OneIn(2))
                            {
                                TryAddObject("Monster", e => e.NPC_Rat());
                            }
                        });
                    }
                    if (r.AllowFeatures)
                    {
                        var roll = new Dice(1, 1);
                        roll.Do(i =>
                        {
                            TryAddFeature("Trap", e => e.Feature_Trap());
                        });
                    }
                    bool TryAddObject<T>(string name, Func<GameEntityBuilders, EntityBuilder<T>> build)
                        where T : PhysicalEntity
                    {
                        while (pointCloud.TryDequeue(out var c))
                        {
                            if (ctx.GetTile(c).Name != TileName.Room)
                                continue;
                            ctx.AddObject(name, c, build);
                            return true;
                        }
                        return false;
                    }
                    bool TryAddFeature<T>(string name, Func<GameEntityBuilders, EntityBuilder<T>> build)
                        where T : Feature
                    {
                        while (pointCloud.TryDequeue(out var c))
                        {
                            if (ctx.GetTile(c).Name != TileName.Room)
                                continue;
                            if (ctx.TryAddFeature(name, c, build))
                                return true;
                        }
                        return false;
                    }
                };
                return room;
            }
        }
    }
}
