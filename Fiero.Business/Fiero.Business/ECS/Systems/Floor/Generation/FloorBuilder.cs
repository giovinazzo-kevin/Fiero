using Fiero.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Linq;

namespace Fiero.Business
{

    public sealed class FloorBuilder
    {
        public readonly Coord Size;

        private readonly GameEntities _entities;
        private readonly GameEntityBuilders _entityBuilders;
        private readonly List<Action<FloorGenerationContext>> _steps;

        internal FloorBuilder(Coord size, GameEntities entities, GameEntityBuilders builders)
        {
            Size = size;
            _entities = entities;
            _entityBuilders = builders;
            _steps = new List<Action<FloorGenerationContext>>();
        }

        public FloorBuilder WithStep(Action<FloorGenerationContext> step)
        {
            _steps.Add(step);
            return this;
        }

        private int CreateStairs(FloorId id, FloorConnection conn, FloorGenerationContext context, HashSet<FloorGenerationContext.Object> hints)
        {
            var pos = new Coord();
            if(id == conn.To) {
                pos = hints.FirstOrDefault(h => h.Name == DungeonObjectName.Upstairs) is { } hint 
                    ? UseHint(hint)
                    : GetRandomPosition();
                return _entityBuilders.Upstairs(conn)
                    .WithPosition(pos)
                    .Build().Id;
            }
            else {
                pos = hints.FirstOrDefault(h => h.Name == DungeonObjectName.Downstairs) is { } hint 
                    ? UseHint(hint)
                    : GetRandomPosition();
                return _entityBuilders.Downstairs(conn)
                    .WithPosition(pos)
                    .Build().Id;
            }

            Coord UseHint(FloorGenerationContext.Object hint)
            {
                hints.Remove(hint);
                return hint.Position;
            }

            Coord GetRandomPosition()
            {
                var validTiles = context.GetAllTiles()
                    .Where(t => t.Name == TileName.Ground)
                    .ToArray();
                return Rng.Random.Choose(validTiles).Position;
            }
        }

        private int CreateEntity(FloorGenerationContext.Object obj)
        {
            var drawable = obj.Name switch {
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

            Drawable CreateShrine()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => _entityBuilders.Shrine().WithPosition(obj.Position).Build()
                )();
            }

            Drawable CreateTrap()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => _entityBuilders.Trap().WithPosition(obj.Position).Build()
                )();
            }

            Drawable CreateChest()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => _entityBuilders.Chest().WithPosition(obj.Position).Build()
                )();
            }

            Drawable CreateConsumable()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                   () => _entityBuilders.Potion(EffectName.Haste).WithPosition(obj.Position).Build(),
                   () => _entityBuilders.Potion(EffectName.Haste).WithPosition(obj.Position).Build(),
                   () => _entityBuilders.Potion(EffectName.Love).WithPosition(obj.Position).Build(),
                   () => _entityBuilders.Potion(EffectName.Rage).WithPosition(obj.Position).Build(),
                   () => _entityBuilders.Scroll(EffectName.Test1).WithPosition(obj.Position).Build(),
                   () => _entityBuilders.Scroll(EffectName.Test2).WithPosition(obj.Position).Build(),
                   () => _entityBuilders.Scroll(EffectName.Test3).WithPosition(obj.Position).Build(),
                   () => _entityBuilders.Scroll(EffectName.Test4).WithPosition(obj.Position).Build()
                )();
            }

            Drawable CreateWeapon()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => _entityBuilders.Sword().WithPosition(obj.Position).Build(),
                    () => _entityBuilders.Bow().WithPosition(obj.Position).Build(),
                    () => _entityBuilders.Staff().WithPosition(obj.Position).Build()
                )();
            }

            Drawable CreateArmor()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => _entityBuilders.LeatherArmor(ArmorSlotName.Head).WithPosition(obj.Position).Build(),
                    () => _entityBuilders.LeatherArmor(ArmorSlotName.Arms).WithPosition(obj.Position).Build(),
                    () => _entityBuilders.LeatherArmor(ArmorSlotName.Legs).WithPosition(obj.Position).Build(),
                    () => _entityBuilders.LeatherArmor(ArmorSlotName.Torso).WithPosition(obj.Position).Build()
                )();
            }

            Drawable CreateItem()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => CreateArmor(),
                    () => CreateWeapon()
                )();
            }

            Drawable CreateBoss()
            {
                return _entityBuilders.NpcGreatKingRat().WithPosition(obj.Position).Build();
            }

            Drawable CreateEnemy()
            {
                var tier = Rng.Random.Choose(
                    MonsterTierName.One,
                    MonsterTierName.One,
                    MonsterTierName.One,
                    MonsterTierName.One,
                    MonsterTierName.One,
                    MonsterTierName.Two,
                    MonsterTierName.Two,
                    MonsterTierName.Two,
                    MonsterTierName.Two,
                    MonsterTierName.Three,
                    MonsterTierName.Three,
                    MonsterTierName.Three,
                    MonsterTierName.Four,
                    MonsterTierName.Four,
                    MonsterTierName.Five
                );
                return Rng.Random.Choose<Func<Drawable>>(
                    () => _entityBuilders.Rat(tier).WithPosition(obj.Position).Build(),
                    () => _entityBuilders.Snake(tier).WithPosition(obj.Position).Build(),
                    () => _entityBuilders.Cat(tier).WithPosition(obj.Position).Build(),
                    () => _entityBuilders.Dog(tier).WithPosition(obj.Position).Build(),
                    () => _entityBuilders.Boar(tier).WithPosition(obj.Position).Build()
                )();
            }
        }

        public Floor Build(FloorId id)
        {
            var floor = new Floor(id, Size);
            var context = new FloorGenerationContext(Size.X, Size.Y);
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
            var tileObjects = objects.TrySelect(e => (_entities.TryGetProxy<Tile>(e, out var t), t));
            foreach (var tile in tileObjects) {
                floor.SetTile(tile);
            }
            context.ForEach(item => {
                if (tileObjects.Any(t => t.Physics.Position == item.Position))
                    return;
                if (item.Name != TileName.None) {
                    floor.SetTile(_entityBuilders.Tile(item.Name).WithPosition(item.Position).Build());
                }
            });
            // Place all features that were added to the context
            var featureObjects = objects.TrySelect(e => (_entities.TryGetProxy<Feature>(e, out var f), f));
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
                .Select(c => CreateStairs(id, c, context, hints));
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
            // Once everything is in its place, build the A* pathfinder
            floor.CreatePathfinder();
            return floor;

            bool IsStairHint(DungeonObjectName n)
            {
                return n == DungeonObjectName.Downstairs ||
                       n == DungeonObjectName.Upstairs;
            }
        }
    }
}
