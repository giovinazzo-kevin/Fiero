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

        private static int CreateEntity(GameEntities entities, FloorGenerationContext.Object obj)
        {
            return obj.Name switch {
                DungeonObjectName.Chest => CreateChest(),
                DungeonObjectName.Shrine => CreateShrine(),
                DungeonObjectName.Trap => CreateTrap(),
                DungeonObjectName.Enemy => CreateEnemy(),
                DungeonObjectName.Boss => CreateBoss(),
                DungeonObjectName.Item => CreateItem(),
                DungeonObjectName.ItemForSale => CreateItem(),
                DungeonObjectName.Consumable => CreateConsumable(),
                DungeonObjectName.ConsumableForSale => CreateConsumable(),
                DungeonObjectName.Downstairs => entities.CreateTile(TileName.Downstairs, new(obj.Position.X, obj.Position.Y)),
                DungeonObjectName.Upstairs => entities.CreateTile(TileName.Upstairs, new(obj.Position.X, obj.Position.Y)),
                _ => entities.CreateEntity("???", "none", new(obj.Position.X, obj.Position.Y))
            };

            int CreateShrine()
            {
                return Rng.Random.Next(0, 1) switch {
                    0 => entities.CreateFeature(FeatureName.Shrine, new(obj.Position.X, obj.Position.Y)),
                    _ => entities.CreateFeature(FeatureName.None, new(obj.Position.X, obj.Position.Y)),
                };
            }

            int CreateTrap()
            {
                return Rng.Random.Next(0, 1) switch {
                    0 => entities.CreateFeature(FeatureName.Trap, new(obj.Position.X, obj.Position.Y)),
                    _ => entities.CreateFeature(FeatureName.None, new(obj.Position.X, obj.Position.Y)),
                };
            }

            int CreateChest()
            {
                return Rng.Random.Next(0, 1) switch {
                    0 => entities.CreateFeature(FeatureName.Chest, new(obj.Position.X, obj.Position.Y), tint: Color.SaddleBrown),
                    _ => entities.CreateFeature(FeatureName.None, new(obj.Position.X, obj.Position.Y)),
                };
            }

            int CreateConsumable()
            {
                return Rng.Random.Next(0, 3) switch {
                    0 => entities.CreateItem(ItemName.Scroll, new(obj.Position.X, obj.Position.Y), tint: Color.SaddleBrown),
                    1 => entities.CreateItem(ItemName.Potion, new(obj.Position.X, obj.Position.Y), tint: Color.Purple),
                    2 => entities.CreateItem(ItemName.Coin, new(obj.Position.X, obj.Position.Y), tint: Color.Gold),
                    _ => entities.CreateItem(ItemName.None, new(obj.Position.X, obj.Position.Y))
                };
            }

            int CreateItem()
            {
                return Rng.Random.Next(0, 6) switch {
                    0 => entities.CreateItem(ItemName.Bow, new(obj.Position.X, obj.Position.Y)),
                    1 => entities.CreateItem(ItemName.Sword, new(obj.Position.X, obj.Position.Y)),
                    2 => entities.CreateItem(ItemName.Wand, new(obj.Position.X, obj.Position.Y)),
                    3 => entities.CreateItem(ItemName.Hat, new(obj.Position.X, obj.Position.Y)),
                    4 => entities.CreateItem(ItemName.Cowl, new(obj.Position.X, obj.Position.Y)),
                    5 => entities.CreateItem(ItemName.Helmet, new(obj.Position.X, obj.Position.Y)),
                    _ => entities.CreateItem(ItemName.None, new(obj.Position.X, obj.Position.Y))
                };
            }

            int CreateBoss()
            {
                return entities.CreateEnemy(new(obj.Position.X, obj.Position.Y), ActorName.Rat, FactionName.Rats, NpcName.GreatKingRat);
            }

            int CreateEnemy()
            {
                return Rng.Random.Next(0, 5) switch {
                    0 => entities.CreateEnemy(new(obj.Position.X, obj.Position.Y), ActorName.Rat, FactionName.Rats),
                    1 => entities.CreateEnemy(new(obj.Position.X, obj.Position.Y), ActorName.Snake, FactionName.Snakes),
                    2 => entities.CreateEnemy(new(obj.Position.X, obj.Position.Y), ActorName.Cat, FactionName.Cats),
                    3 => entities.CreateEnemy(new(obj.Position.X, obj.Position.Y), ActorName.Dog, FactionName.Dogs),
                    4 => entities.CreateEnemy(new(obj.Position.X, obj.Position.Y), ActorName.Boar, FactionName.Boars),
                    _ => entities.CreateEnemy(new(obj.Position.X, obj.Position.Y), ActorName.None, FactionName.Players)
                };

            }
        }

        public Floor Build(GameEntities entities)
        {
            var context = new FloorGenerationContext(Size.X, Size.Y);
            foreach (var step in _steps) {
                step(context);
            }
            var floor = new Floor(entities, context);
            var objects = context.GetObjects().Select(o => CreateEntity(entities, o))
                .ToList();
            var tileObjects = objects.TrySelect(e => (entities.TryGetProxy<Tile>(e, out var t), t));
            var actorObjects = objects.TrySelect(e => (entities.TryGetProxy<Actor>(e, out var a), a));
            var itemObjects = objects.TrySelect(e => (entities.TryGetProxy<Item>(e, out var i), i));
            var featureObjects = objects.TrySelect(e => (entities.TryGetProxy<Feature>(e, out var f), f));
            context.ForEach(item => {
                if (tileObjects.Any(t => t.Physics.Position == item.P))
                    return;
                if (item.Tile != TileName.None) {
                    floor.SetTile(entities.CreateTile(item.Tile, item.P));
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
