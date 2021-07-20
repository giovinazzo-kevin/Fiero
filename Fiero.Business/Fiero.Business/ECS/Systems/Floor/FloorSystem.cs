using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Unconcern.Common;

namespace Fiero.Business
{
    public class FloorSystem : EcsSystem
    {
        protected readonly Dictionary<FloorId, Floor> Floors;

        public readonly GameEntities Entities;
        public readonly GameEntityBuilders EntityBuilders;
        public readonly GameDataStore Store;

        public FloorSystem(EventBus bus, GameEntities entities, GameEntityBuilders entityBuilders, GameDataStore store)
            : base(bus)
        {
            Entities = entities;
            Store = store;
            EntityBuilders = entityBuilders;
            Floors = new();
        }

        public void Reset()
        {
            foreach (var floor in Floors.Values) {

                foreach (var cell in floor.Cells.Values) {
                    Entities.FlagEntityForRemoval(cell.Tile.Id);
                    foreach (var actor in cell.Actors) {
                        Entities.FlagEntityForRemoval(actor.Id);
                    }
                }
            }
            Entities.RemoveFlagged(propagate: true);
            Floors.Clear();
        }

        public bool TryGetFloor(FloorId id, out Floor floor) => Floors.TryGetValue(id, out floor);
        public Floor GetFloor(FloorId id) => TryGetFloor(id, out var floor) ? floor : null;
        public IEnumerable<Floor> GetAllFloors() => Floors.Values;

        public void AddFloor(FloorId id, Coord size, Func<FloorBuilder, FloorBuilder> configure)
        {
            Floors.Add(id, configure(new FloorBuilder(size))
                .Build(id, Entities, EntityBuilders));
        }

        public bool TryGetCellAt(FloorId id, Coord pos, out MapCell cell)
        {
            cell = default;
            return TryGetFloor(id, out var floor) && floor.Cells.TryGetValue(pos, out cell);
        }
        public MapCell GetCellAt(FloorId id, Coord pos) => TryGetCellAt(id, pos, out var cell) ? cell : null;

        public IEnumerable<Drawable> GetDrawables(FloorId id) => TryGetFloor(id, out var floor)
            ? floor.GetDrawables()
            : Enumerable.Empty<Drawable>();
        public IEnumerable<Tile> GetAllTiles(FloorId id) => TryGetFloor(id, out var floor)
            ? floor.Cells.Values.Select(c => c.Tile)
            : Enumerable.Empty<Tile>();
        public bool TryGetTileAt(FloorId id, Coord pos, out Tile tile)
        {
            tile = default;
            if(TryGetCellAt(id, pos, out var cell)) {
                tile = cell.Tile;
                return true;
            }
            return false;
        }
        public Tile GetTileAt(FloorId id, Coord pos) => TryGetCellAt(id, pos, out var cell) ? cell.Tile : null;
        public void SetTileAt(FloorId id, Coord pos, TileName tile)
        {
            if(TryGetFloor(id, out var floor)) {
                if (TryGetCellAt(id, pos, out var old)) {
                    Entities.FlagEntityForRemoval(old.Tile.Id);
                }
                var entity = EntityBuilders.Tile(tile)
                    .WithPosition(pos)
                    .Build();
                floor.SetTile(entity);
            }
        }
        public IEnumerable<MapCell> GetNeighborsAt(FloorId id, Coord pos)
        {
            if (TryGetCellAt(id, new(pos.X - 1, pos.Y - 1), out var n1))
                yield return n1;
            if (TryGetCellAt(id, new(pos.X, pos.Y - 1), out var n2))
                yield return n2;
            if (TryGetCellAt(id, new(pos.X + 1, pos.Y - 1), out var n3))
                yield return n3;
            if (TryGetCellAt(id, new(pos.X - 1, pos.Y), out var n4))
                yield return n4;
            if (TryGetCellAt(id, new(pos.X, pos.Y), out var n5))
                yield return n5;
            if (TryGetCellAt(id, new(pos.X + 1, pos.Y), out var n6))
                yield return n6;
            if (TryGetCellAt(id, new(pos.X - 1, pos.Y + 1), out var n7))
                yield return n7;
            if (TryGetCellAt(id, new(pos.X, pos.Y + 1), out var n8))
                yield return n8;
            if (TryGetCellAt(id, new(pos.X + 1, pos.Y + 1), out var n9))
                yield return n9;
        }
        public bool TryGetClosestFreeTile(FloorId id, Coord pos, out Tile closest, float maxDistance = 10)
        {
            closest = default;
            if (TryGetCellAt(id, pos, out var closestCell)
                && !closestCell.Actors.Any()
                && !closestCell.Items.Any()
                && !closestCell.Features.Any(f => f.FeatureProperties.BlocksMovement)) {
                closest = closestCell.Tile;
                return true;
            }
            if (--maxDistance <= 0)
                return false;
            var neighbors = GetNeighborsAt(id, pos)
                .Select(c => c.Tile)
                .Where(n => !n.TileProperties.BlocksMovement)
                .OrderBy(n => Rng.Random.NextDouble())
                .ToList();
            if (neighbors.All(n => n.DistanceFrom(pos) > maxDistance))
                return false;
            foreach (var n in neighbors) {
                if (TryGetClosestFreeTile(id, n.Physics.Position, out closest, maxDistance)) {
                    return true;
                }
            }
            return false;
        }
        public IEnumerable<Actor> GetAllActors(FloorId id) => TryGetFloor(id, out var floor)
            ? floor.Cells.Values.SelectMany(c => c.Actors)
            : Enumerable.Empty<Actor>();
        public IEnumerable<Actor> GetActorsAt(FloorId id, Coord pos) => TryGetFloor(id, out var floor)
            ? floor.Cells.TryGetValue(pos, out var cell)
                ? cell.Actors
                : Enumerable.Empty<Actor>()
            : Enumerable.Empty<Actor>();
        public bool AddActor(FloorId id, Actor actor)
        {
            if (TryGetFloor(id, out var floor)) {
                floor.AddActor(actor);
                return true;
            }
            return false;
        }
        public bool RemoveActor(FloorId id, Actor actor)
        {
            if (TryGetFloor(id, out var floor)) {
                floor.RemoveActor(actor);
                return true;
            }
            return false;
        }

        public IEnumerable<Item> GetAllItems(FloorId id) => TryGetFloor(id, out var floor)
            ? floor.Cells.Values.SelectMany(c => c.Items)
            : Enumerable.Empty<Item>();
        public IEnumerable<Item> GetItemsAt(FloorId id, Coord pos) => TryGetFloor(id, out var floor)
            ? floor.Cells.TryGetValue(pos, out var cell)
                ? cell.Items
                : Enumerable.Empty<Item>()
            : Enumerable.Empty<Item>();
        public bool AddItem(FloorId id, Item item)
        {
            if (TryGetFloor(id, out var floor)) {
                floor.AddItem(item);
                return true;
            }
            return false;
        }
        public bool RemoveItem(FloorId id, Item item)
        {
            if (TryGetFloor(id, out var floor)) {
                floor.RemoveItem(item);
                return true;
            }
            return false;
        }

        public IEnumerable<Feature> GetAllFeatures(FloorId id) => TryGetFloor(id, out var floor)
            ? floor.Cells.Values.SelectMany(c => c.Features)
            : Enumerable.Empty<Feature>();
        public IEnumerable<Feature> GetFeaturesAt(FloorId id, Coord pos) => TryGetFloor(id, out var floor)
            ? floor.Cells.TryGetValue(pos, out var cell)
                ? cell.Features
                : Enumerable.Empty<Feature>()
            : Enumerable.Empty<Feature>();
        public bool AddFeature(FloorId id, Feature feature)
        {
            if (TryGetFloor(id, out var floor)) {
                floor.AddFeature(feature);
                return true;
            }
            return false;
        }
        public bool RemoveFeature(FloorId id, Feature feature)
        {
            if (TryGetFloor(id, out var floor)) {
                floor.RemoveFeature(feature);
                return true;
            }
            return false;
        }

        public void RecalculateFov(Actor a)
        {
            if(a.Fov != null && TryGetFloor(a.FloorId(), out var floor)) {
                a.Fov.VisibleTiles.Clear();
                a.Fov.VisibleTiles.UnionWith(floor.CalculateFov(a.Physics.Position, a.Fov.Radius));
                a.Fov.KnownTiles.UnionWith(a.Fov.VisibleTiles);
            }
        }

        public bool IsLineOfSightBlocked(FloorId id, Coord a, Coord b)
            => !TryGetFloor(id, out var floor) || Utils.BresenhamPoints(a, b).Any(p => !floor.Cells.TryGetValue(p, out var cell) || !cell.Tile.IsWalkable(null));
    }
}
