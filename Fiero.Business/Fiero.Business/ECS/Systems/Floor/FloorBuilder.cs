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
        public readonly Coord TileSize;

        private readonly List<Action<FloorGenerationContext>> _steps;

        internal FloorBuilder(Coord size, Coord tileSize)
        {
            Size = size;
            TileSize = tileSize;
            _steps = new List<Action<FloorGenerationContext>>();
        }

        public FloorBuilder WithStep(Action<FloorGenerationContext> step)
        {
            _steps.Add(step);
            return this;
        }

        private static int CreateEntity(GameEntityBuilders entities, FloorGenerationContext.Object obj)
        {
            var drawable = obj.Name switch {
                DungeonObjectName.Chest => CreateChest(),
                DungeonObjectName.Shrine => CreateShrine(),
                DungeonObjectName.Trap => CreateTrap(),
                DungeonObjectName.Enemy => CreateEnemy(),
                DungeonObjectName.Boss => CreateBoss(),
                DungeonObjectName.Item => CreateItem(),
                DungeonObjectName.ItemForSale => CreateItem(),
                DungeonObjectName.Consumable => CreateConsumable(),
                DungeonObjectName.ConsumableForSale => CreateConsumable(),
                DungeonObjectName.Downstairs => entities.Tile(TileName.Downstairs).WithPosition(obj.Position).Build(),
                DungeonObjectName.Upstairs => entities.Tile(TileName.Upstairs).WithPosition(obj.Position).Build(),
                _ => throw new NotImplementedException()
            };
            
            return drawable.Id;

            Drawable CreateShrine()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => entities.Shrine().WithPosition(obj.Position).Build()
                )();
            }

            Drawable CreateTrap()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => entities.Trap().WithPosition(obj.Position).Build()
                )();
            }

            Drawable CreateChest()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => entities.Chest().WithPosition(obj.Position).Build()
                )();
            }

            Drawable CreateConsumable()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                   () => entities.Potion(EffectName.Haste).WithPosition(obj.Position).Build(),
                   () => entities.Scroll(EffectName.Haste).WithPosition(obj.Position).Build(),
                   () => entities.Scroll(EffectName.Love).WithPosition(obj.Position).Build(),
                   () => entities.Scroll(EffectName.Rage).WithPosition(obj.Position).Build(),
                   () => entities.Scroll(EffectName.Test1).WithPosition(obj.Position).Build(),
                   () => entities.Scroll(EffectName.Test2).WithPosition(obj.Position).Build(),
                   () => entities.Scroll(EffectName.Test3).WithPosition(obj.Position).Build(),
                   () => entities.Scroll(EffectName.Test4).WithPosition(obj.Position).Build()
                )();
            }

            Drawable CreateWeapon()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => entities.Sword().WithPosition(obj.Position).Build(),
                    () => entities.Bow().WithPosition(obj.Position).Build(),
                    () => entities.Staff().WithPosition(obj.Position).Build()
                )();
            }

            Drawable CreateArmor()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => entities.Sword().WithPosition(obj.Position).Build()
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
                return entities.NpcGreatKingRat().WithPosition(obj.Position).Build();
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
                    () => entities.Rat(tier).WithPosition(obj.Position).Build(),
                    () => entities.Snake(tier).WithPosition(obj.Position).Build(),
                    () => entities.Cat(tier).WithPosition(obj.Position).Build(),
                    () => entities.Dog(tier).WithPosition(obj.Position).Build(),
                    () => entities.Boar(tier).WithPosition(obj.Position).Build()
                )();
            }
        }

        public Floor Build(GameEntities entities, GameEntityBuilders builders)
        {
            var context = new FloorGenerationContext(Size.X, Size.Y);
            for (int x = 0; x < Size.X; x++) {
                for (int y = 0; y < Size.Y; y++) {
                    context.Set(x, y, TileName.Wall);
                }
            }
            foreach (var step in _steps) {
                step(context);
            }
            var floor = new Floor(entities, context);
            var objects = context.GetObjects().Select(o => CreateEntity(builders, o))
                .ToList();
            var tileObjects = objects.TrySelect(e => (entities.TryGetProxy<Tile>(e, out var t), t));
            var actorObjects = objects.TrySelect(e => (entities.TryGetProxy<Actor>(e, out var a), a));
            var itemObjects = objects.TrySelect(e => (entities.TryGetProxy<Item>(e, out var i), i));
            var featureObjects = objects.TrySelect(e => (entities.TryGetProxy<Feature>(e, out var f), f));
            context.ForEach(item => {
                if (tileObjects.Any(t => t.Physics.Position == item.P))
                    return;
                if (item.Tile != TileName.None) {
                    floor.SetTile(builders.Tile(item.Tile).WithPosition(item.P).Build().Id);
                }
            });
            foreach (var tile in tileObjects) {
                floor.SetTile(tile.Id);
            }
            foreach (var actor in actorObjects) {
                floor.AddActor(actor.Id);
            }
            foreach (var item in itemObjects) {
                floor.AddItem(item.Id);
            }
            foreach (var feature in featureObjects) {
                floor.AddFeature(feature.Id);
            }
            // Once everything is in its place, build the pathfinder and assign it to each actor
            floor.CreatePathfinder();
            return floor;
        }
    }
}
