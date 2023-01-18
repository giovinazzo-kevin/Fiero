using Fiero.Core;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class SewersBranchGenerator : BranchGenerator
    {
        static bool GKRAdded = false;

        protected virtual EntityBuilder<Actor> GenerateMonster(FloorId floorId, GameEntityBuilders builder)
        {
            var D = floorId.Depth;
            if (!GKRAdded)
            {
                GKRAdded = true;
                return builder.Boss_NpcGreatKingRat();
            }

            return Rng.Random.ChooseWeighted(new[] {
                (builder.NPC_Rat(), D * 0 + 100f)
            });
        }

        public override Floor GenerateFloor(FloorId floorId, Coord size, FloorBuilder builder)
        {
            var subdivisions = floorId.Depth switch
            {
                var x when x < 2 => new Coord(1, 1),
                var x when x < 5 => new Coord(2, 2),
                var x when x < 10 => new Coord(4, 4),
                _ => new Coord(4, 4),
            };
            var sectors = new IntRect(new(), size - new Coord(1, 1)).Subdivide(subdivisions).ToList();

            var roomSectors = sectors.Select(s => RoomSector.Create(s, CreateRoom, 1)).ToList();
            var interCorridors = RoomSector.GenerateInterSectorCorridors(roomSectors, 1).ToList();

            return builder
                .WithStep(ctx =>
                {
                    foreach (var sector in roomSectors)
                    {
                        ctx.Draw(sector);
                    }
                    foreach (var corridor in interCorridors)
                    {
                        ctx.Draw(corridor);
                    }
                })
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
                    (() => new ShrineRoom(),    2.5f),
                    (() => new TreasureRoom(),  50.0f),
                    (() => new EmptyRoom() ,   92.5f)
                })();

                room.Drawn += (r, ctx) =>
                {
                    var area = r.GetRects().Count(); // Chances are not actually per-room but per room square
                    var pointCloud = new Queue<Coord>(r.GetPointCloud().Shuffle(Rng.Random));
                    if (r.AllowMonsters)
                    {
                        Rng.Random.Roll(1, (floorId.Depth / 10 + 1) * area, i =>
                        {
                            TryAddObject("Monster", e => GenerateMonster(floorId, e));
                        });
                    }
                    bool TryAddObject<T>(string name, Func<GameEntityBuilders, EntityBuilder<T>> build)
                        where T : PhysicalEntity
                    {
                        if (pointCloud.TryDequeue(out var c))
                        {
                            ctx.AddObject(name, c, build);
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
