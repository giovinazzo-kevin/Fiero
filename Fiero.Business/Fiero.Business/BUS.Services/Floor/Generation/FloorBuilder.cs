using Fiero.Core;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;

namespace Fiero.Business
{

    [TransientDependency]
    public sealed class FloorBuilder
    {
        private readonly GameEntities _entities;
        private readonly GameEntityBuilders _entityBuilders;
        private readonly List<Action<FloorGenerationContext>> _steps;

        public FloorBuilder(GameEntities entities, GameEntityBuilders builders)
        {
            _entities = entities;
            _entityBuilders = builders;
            _steps = new List<Action<FloorGenerationContext>>();
        }

        public FloorBuilder WithStep(Action<FloorGenerationContext> step)
        {
            _steps.Add(step);
            return this;
        }

        private int CreateStairs(FloorId id, FloorConnection conn, FloorGenerationContext context, HashSet<ObjectDef> hints)
        {
            var pos = new Coord();
            if(id == conn.To) {
                pos = hints.FirstOrDefault(h => h.Name == DungeonObjectName.Upstairs) is { } hint 
                    ? UseHint(hint)
                    : GetRandomPosition();
                return _entityBuilders.Feature_Upstairs(conn)
                    .WithPosition(pos)
                    .Build().Id;
            }
            else {
                pos = hints.FirstOrDefault(h => h.Name == DungeonObjectName.Downstairs) is { } hint 
                    ? UseHint(hint)
                    : GetRandomPosition();
                return _entityBuilders.Feature_Downstairs(conn)
                    .WithPosition(pos)
                    .Build().Id;
            }

            Coord UseHint(ObjectDef hint)
            {
                hints.Remove(hint);
                return hint.Position;
            }

            Coord GetRandomPosition()
            {
                var validTiles = context.GetAllTiles()
                    .Where(t => t.Name == TileName.Room && !context.GetObjects().Any(o => o.Position == t.Position))
                    .ToArray();
                return Rng.Random.Choose(validTiles).Position;
            }
        }

        private int CreateEntity(ObjectDef obj)
        {
            var drawable = obj.Name switch {
                DungeonObjectName.Door => CreateDoor(),
                DungeonObjectName.Chest => CreateChest(),
                DungeonObjectName.Shrine => CreateShrine(),
                DungeonObjectName.Trap => CreateTrap(),
                DungeonObjectName.Enemy => CreateEnemy(),
                DungeonObjectName.Boss => CreateBoss(),
                DungeonObjectName.Item => CreateItem(),
                DungeonObjectName.Consumable => CreateConsumable(),
                _ => throw new NotImplementedException()
            };
            
            return drawable.Id;

            DrawableEntity CreateShrine()
            {
                return Rng.Random.Choose<Func<DrawableEntity>>(
                    () => _entityBuilders.Feature_Shrine().WithPosition(obj.Position).Build()
                )();
            }

            DrawableEntity CreateDoor()
            {
                return Rng.Random.Choose<Func<DrawableEntity>>(
                    () => _entityBuilders.Feature_Door().WithPosition(obj.Position).Build()
                )();
            }

            DrawableEntity CreateTrap()
            {
                return Rng.Random.Choose<Func<DrawableEntity>>(
                    () => _entityBuilders.Feature_Trap().WithPosition(obj.Position).Build()
                )();
            }

            DrawableEntity CreateChest()
            {
                return Rng.Random.Choose<Func<DrawableEntity>>(
                    () => _entityBuilders.Feature_Chest().WithPosition(obj.Position).Build()
                )();
            }

            DrawableEntity CreateConsumable()
            {
                return Rng.Random.Choose<Func<DrawableEntity>>(
                   () => _entityBuilders.Potion_OfConfusion().WithPosition(obj.Position).Build(),
                   () => _entityBuilders.Potion_OfSleep().WithPosition(obj.Position).Build(),
                   () => _entityBuilders.Wand_OfConfusion(Rng.Random.Between(4, 8)).WithPosition(obj.Position).Build(),
                   () => _entityBuilders.Wand_OfSleep(Rng.Random.Between(4, 8)).WithPosition(obj.Position).Build(),
                   () => _entityBuilders.Scroll_OfMassConfusion().WithPosition(obj.Position).Build(),
                   () => _entityBuilders.Scroll_OfMassSleep().WithPosition(obj.Position).Build(),
                   () => _entityBuilders.Throwable_Rock(Rng.Random.Between(1, 10)).WithPosition(obj.Position).Build(),
                   () => _entityBuilders.Resource_Gold(Rng.Random.Between(1, 100)).WithPosition(obj.Position).Build()
                )();
            }

            DrawableEntity CreateWeapon()
            {
                return Rng.Random.Choose<Func<DrawableEntity>>(
                    () => _entityBuilders.Weapon_Sword().WithPosition(obj.Position).Build()
                )();
            }

            DrawableEntity CreateItem()
            {
                return Rng.Random.Choose<Func<DrawableEntity>>(
                    () => CreateWeapon()
                )();
            }

            DrawableEntity CreateBoss()
            {
                return _entityBuilders.Boss_NpcGreatKingRat().WithPosition(obj.Position).Build();
            }

            DrawableEntity CreateEnemy()
            {
                return Rng.Random.Choose<Func<DrawableEntity>>(
                    () => _entityBuilders.NPC_Rat().WithPosition(obj.Position).Build(),
                    () => _entityBuilders.NPC_RatKnight().WithPosition(obj.Position).Build(),
                    () => _entityBuilders.NPC_RatArcher().WithPosition(obj.Position).Build(),
                    () => _entityBuilders.NPC_RatWizard().WithPosition(obj.Position).Build(),
                    () => _entityBuilders.NPC_RatMonk().WithPosition(obj.Position).Build(),
                    () => _entityBuilders.NPC_RatPugilist().WithPosition(obj.Position).Build(),
                    () => _entityBuilders.NPC_RatThief().WithPosition(obj.Position).Build(),
                    () => _entityBuilders.NPC_RatOutcast().WithPosition(obj.Position).Build(),
                    () => _entityBuilders.NPC_RatArsonist().WithPosition(obj.Position).Build(),
                    () => _entityBuilders.NPC_RatMerchant().WithPosition(obj.Position).Build(),
                    () => _entityBuilders.NPC_SandSnake().WithPosition(obj.Position).Build(),
                    () => _entityBuilders.NPC_Cobra().WithPosition(obj.Position).Build(),
                    () => _entityBuilders.NPC_Boa().WithPosition(obj.Position).Build()
                    //() => _entityBuilders.NPC_Snake().WithPosition(obj.Position).Build(),
                    //() => _entityBuilders.NPC_Cat().WithPosition(obj.Position).Build(),
                    //() => _entityBuilders.NPC_Dog().WithPosition(obj.Position).Build(),
                    //() => _entityBuilders.NPC_Boar().WithPosition(obj.Position).Build()
                )();
            }
        }

        private EntityBuilder<Tile> BuildTile(TileDef tile)
        {
            var ret = tile.Name switch {
                TileName.Wall => _entityBuilders.Tile_Wall(),
                TileName.Room => _entityBuilders.Tile_Room(),
                TileName.Corridor => _entityBuilders.Tile_Corridor(),
                _ => _entityBuilders.Tile_Unimplemented()
            };
            if(tile.Color is { } tint) {
                ret = ret.WithColor(tint);
            }
            return ret;
        }

        public Floor Build(FloorId id, Coord size)
        {
            var floor = new Floor(id, size);
            var context = new FloorGenerationContext(size.X, size.Y);
            // Run user steps, initializing the context
            foreach (var step in _steps) {
                step(context);
            }
            // Get all objects that were added to the context, but exclude portals and stairs which need special handling
            var objects = context.GetObjects()
                .Where(o => !IsStairHint(o.Name))
                .Select(o => CreateEntity(o))
                .ToList();
            // Place all tiles that were set in the context, including objects that eventually resolve to tiles
            var tileObjects = objects.TrySelect(e => (_entities.TryGetProxy<Tile>(e, out var t), t))
                .ToList();
            foreach (var tile in tileObjects) {
                floor.SetTile(tile);
            }
            context.ForEach(item => {
                if (tileObjects.Any(t => t.Position() == item.Position))
                    return;
                if (item.Name != TileName.None) {
                    floor.SetTile(BuildTile(item)
                        .Tweak<PhysicsComponent>(x => x.Position = item.Position)
                    .Build());
                }
            });
            // Place all features that were added to the context
            var featureObjects = objects.TrySelect(e => (_entities.TryGetProxy<Feature>(e, out var f), f))
                .ToList();
            foreach (var feature in featureObjects) {
                floor.AddFeature(feature);
            }
            // Get the list of stair objects from the context and treat it as a grab bag of "hints",
            // meaning if no hint is found, a random valid ground tile's position is chosen instead.
            // this ensures that a level always ends up having the right amount of stairs,
            // while giving some artistic liberty to the level designer who cares about stairs.
            var hints = context.GetObjects()
                .Where(o => IsStairHint(o.Name))
                .ToHashSet();
            var stairs = context.GetConnections()
                .Select(c => CreateStairs(id, c, context, hints))
                .ToList();
            // Stairs are features, not tiles, because you can use them
            var stairObjects = stairs.TrySelect(e => (_entities.TryGetProxy<Feature>(e, out var f), f));
            foreach (var stair in stairObjects) {
                floor.AddFeature(stair);
            }
            // Place all items that were added to the context
            var itemObjects = objects.TrySelect(e => (_entities.TryGetProxy<Item>(e, out var i), i));
            foreach (var item in itemObjects) {
                floor.AddItem(item);
            }
            // Spawn all enemies and actors that were added to the context
            var actorObjects = objects.TrySelect(e => (_entities.TryGetProxy<Actor>(e, out var a), a));
            foreach (var actor in actorObjects) {
                floor.AddActor(actor);
            }
            return floor;

            bool IsStairHint(DungeonObjectName n)
            {
                return n == DungeonObjectName.Downstairs ||
                       n == DungeonObjectName.Upstairs;
            }
        }
    }
}
