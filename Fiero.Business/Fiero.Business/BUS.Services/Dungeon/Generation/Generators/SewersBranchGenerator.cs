using Fiero.Core;
using Fiero.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class SewersBranchGenerator : BranchGenerator
    {
        static bool GKRAdded = false;

        public override Floor GenerateFloor(FloorId floorId, FloorBuilder builder)
        {
            var (size, subdivisions) = floorId.Depth switch
            {
                _ => (new Coord(51, 51), new Coord(2, 2)),
            };
            var roomSectors = RoomSector.CreateTiling((size - Coord.PositiveOne) / subdivisions, subdivisions, CreateRoom, new Dice(1, 2))
                .ToList();
            var corridors = RoomSector.GenerateInterSectorCorridors(roomSectors, new Dice(1, 2))
                .ToList();

            //var numSecrets = new Dice(2, 2);
            //foreach (var n in numSecrets.Roll())
            //{
            //    var sector = Rng.Random.Choose(roomSectors);
            //    sector.MarkSecretCorridors(n);
            //}
            foreach (var r in roomSectors)
            {
                r.MarkActiveCorridors(corridors);
            }

            var tree = RoomTree.Build(
                roomSectors.SelectMany(s => s.Rooms).ToArray(),
                corridors.Concat(roomSectors.SelectMany(s => s.Corridors)).ToArray()
            );

            return builder
                .WithStep(tree.Draw)
                .WithStep(ctx =>
                {
                    foreach (var conn in ctx.GetConnections())
                    {
                        // Add upstairs and downstairs to respective floors
                        var emptyTiles = ctx.GetEmptyTiles()
                            .Where(t => t.Name == TileName.Room)
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

            Room CreateRoom()
            {
                Room room = Rng.Random.ChooseWeighted(new (Func<Room>, float)[] {
                    (() => new ShrineRoom(),    0.5f),
                    (() => new TreasureRoom(),  1.0f),
                    //(() => new CrampedRoom() ,  44.25f),
                    (() => new EmptyRoom() ,    44.25f)
                })();

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
                                // TryAddObject("Monster", e => GenerateMonster(floorId, e));
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
