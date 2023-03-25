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

        protected virtual EntityBuilder<Actor> GenerateMonster(FloorId floorId, GameEntityBuilders builder)
        {
            var D = floorId.Depth;
            if (!GKRAdded)
            {
                GKRAdded = true;
                return builder.Boss_NpcGreatKingRat();
            }

            return Rng.Random.ChooseWeighted(new[] {
                (builder.NPC_RatMerchant(), D * 0 + 1f),
                (builder.NPC_Rat(), D * 0 + 100f),
                (builder.NPC_RatCheese(), D * 0 + 10f),
                (builder.NPC_RatArcher(), D * 1 + 50f),
                (builder.NPC_RatThief(), D * 1 + 25f),
                (builder.NPC_RatMonk(), D * 2 + 30f),
                (builder.NPC_RatPugilist(), D * 2 + 20f),
                (builder.NPC_RatWizard(), D * 3 + 40f),
                (builder.NPC_RatArsonist(), D * 5 + 10f),
            });
        }

        public override Floor GenerateFloor(FloorId floorId, FloorBuilder builder)
        {
            var (size, subdivisions) = floorId.Depth switch
            {
                _ => (new Coord(50, 50), new Coord(2, 2)),
            };
            var roomSectors = RoomSector.CreateTiling((size - Coord.PositiveOne) / subdivisions, subdivisions, CreateRoom, new Dice(1, 2)).ToList();
            var interCorridors = RoomSector.GenerateInterSectorCorridors(roomSectors, new Dice(1, 2)).ToList();

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
                    (() => new ShrineRoom(),    0.5f),
                    (() => new TreasureRoom(),  1.0f),
                    (() => new EmptyRoom() ,   98.5f)
                })();

                room.Drawn += (r, ctx) =>
                {
                    var area = r.GetRects().Count(); // Chances are not actually per-room but per room square
                    var pointCloud = new Queue<Coord>(r.GetPointCloud().Shuffle(Rng.Random));
                    if (r.AllowMonsters)
                    {
                        var roll = new Dice(1, 3);
                        roll.Do(i =>
                        {
                            if (Chance.OneIn(10))
                            {
                                TryAddObject("Monster", e => GenerateMonster(floorId, e));
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
