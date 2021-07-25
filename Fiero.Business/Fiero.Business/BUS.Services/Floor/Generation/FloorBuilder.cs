﻿using Fiero.Core;
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
        private readonly GameColors<ColorName> _colors;
        private readonly List<Action<FloorGenerationContext>> _steps;

        public FloorBuilder(GameEntities entities, GameEntityBuilders builders, GameColors<ColorName> colors)
        {
            _entities = entities;
            _entityBuilders = builders;
            _colors = colors;
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
                return _entityBuilders.Upstairs(conn)
                    .WithPhysics(pos)
                    .Build().Id;
            }
            else {
                pos = hints.FirstOrDefault(h => h.Name == DungeonObjectName.Downstairs) is { } hint 
                    ? UseHint(hint)
                    : GetRandomPosition();
                return _entityBuilders.Downstairs(conn)
                    .WithPhysics(pos)
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
                    .Where(t => t.Name == TileName.Ground && !context.GetObjects().Any(o => o.Position == t.Position))
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

            Drawable CreateShrine()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => _entityBuilders.Shrine().WithPhysics(obj.Position).Build()
                )();
            }

            Drawable CreateDoor()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => _entityBuilders.Door().WithPhysics(obj.Position).Build()
                )();
            }

            Drawable CreateTrap()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => _entityBuilders.Trap().WithPhysics(obj.Position).Build()
                )();
            }

            Drawable CreateChest()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => _entityBuilders.Chest().WithPhysics(obj.Position).Build()
                )();
            }

            Drawable CreateConsumable()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                   () => _entityBuilders.Potion(EffectName.Haste).WithPhysics(obj.Position).Build(),
                   () => _entityBuilders.Potion(EffectName.Haste).WithPhysics(obj.Position).Build(),
                   () => _entityBuilders.Potion(EffectName.Love).WithPhysics(obj.Position).Build(),
                   () => _entityBuilders.Potion(EffectName.Rage).WithPhysics(obj.Position).Build(),
                   () => _entityBuilders.Scroll(EffectName.Test1).WithPhysics(obj.Position).Build(),
                   () => _entityBuilders.Scroll(EffectName.Test2).WithPhysics(obj.Position).Build(),
                   () => _entityBuilders.Scroll(EffectName.Test3).WithPhysics(obj.Position).Build(),
                   () => _entityBuilders.Scroll(EffectName.Test4).WithPhysics(obj.Position).Build()
                )();
            }

            Drawable CreateWeapon()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => _entityBuilders.Sword().WithPhysics(obj.Position).Build(),
                    () => _entityBuilders.Bow().WithPhysics(obj.Position).Build(),
                    () => _entityBuilders.Staff().WithPhysics(obj.Position).Build()
                )();
            }

            Drawable CreateArmor()
            {
                return Rng.Random.Choose<Func<Drawable>>(
                    () => _entityBuilders.LeatherArmor(ArmorSlotName.Head).WithPhysics(obj.Position).Build(),
                    () => _entityBuilders.LeatherArmor(ArmorSlotName.Arms).WithPhysics(obj.Position).Build(),
                    () => _entityBuilders.LeatherArmor(ArmorSlotName.Legs).WithPhysics(obj.Position).Build(),
                    () => _entityBuilders.LeatherArmor(ArmorSlotName.Torso).WithPhysics(obj.Position).Build()
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
                return _entityBuilders.NpcGreatKingRat().WithPhysics(obj.Position).Build();
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
                    () => _entityBuilders.Rat(tier).WithPhysics(obj.Position).Build(),
                    () => _entityBuilders.Snake(tier).WithPhysics(obj.Position).Build(),
                    () => _entityBuilders.Cat(tier).WithPhysics(obj.Position).Build(),
                    () => _entityBuilders.Dog(tier).WithPhysics(obj.Position).Build(),
                    () => _entityBuilders.Boar(tier).WithPhysics(obj.Position).Build()
                )();
            }
        }

        private EntityBuilder<Tile> BuildTile(TileDef tile)
        {
            var ret = tile.Name switch {
                TileName.Wall => _entityBuilders.WallTile(),
                TileName.Ground => _entityBuilders.GroundTile(),
                _ => _entityBuilders.UnimplementedTile()
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
            var tileObjects = objects.TrySelect(e => (_entities.TryGetProxy<Tile>(e, out var t), t));
            foreach (var tile in tileObjects) {
                floor.SetTile(tile);
            }
            context.ForEach(item => {
                if (tileObjects.Any(t => t.Physics.Position == item.Position))
                    return;
                if (item.Name != TileName.None) {
                    floor.SetTile(BuildTile(item)
                        .Tweak<PhysicsComponent>(x => x.Position = item.Position)
                    .Build());
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