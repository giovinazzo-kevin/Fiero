using Fiero.Core;
using LightInject;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Unconcern.Common;

namespace Fiero.Business
{

    public partial class FloorSystem : EcsSystem
    {
        protected readonly Dictionary<FloorId, Floor> Floors;

        public readonly IServiceFactory ServiceProvider;
        public readonly GameEntities Entities;
        public readonly GameEntityBuilders EntityBuilders;
        public readonly GameDataStore Store;

        public readonly SystemEvent<FloorSystem, TileChangedEvent> TileChanged;
        public readonly SystemEvent<FloorSystem, ActorChangedEvent> ActorAdded;
        public readonly SystemEvent<FloorSystem, ActorChangedEvent> ActorRemoved;
        public readonly SystemEvent<FloorSystem, ItemChangedEvent> ItemAdded;
        public readonly SystemEvent<FloorSystem, ItemChangedEvent> ItemRemoved;
        public readonly SystemEvent<FloorSystem, FeatureChangedEvent> FeatureAdded;
        public readonly SystemEvent<FloorSystem, FeatureChangedEvent> FeatureRemoved;

        public FloorSystem(EventBus bus, GameEntities entities, GameEntityBuilders entityBuilders, GameDataStore store, IServiceFactory sp)
            : base(bus)
        {
            Entities = entities;
            Store = store;
            EntityBuilders = entityBuilders;
            ServiceProvider = sp;
            Floors = new();

            TileChanged = new(this, nameof(TileChanged));
            ActorAdded = new(this, nameof(ActorAdded));
            ActorRemoved = new(this, nameof(ActorRemoved));
            ItemAdded = new(this, nameof(ItemAdded));
            ItemRemoved = new(this, nameof(ItemRemoved));
            FeatureAdded = new(this, nameof(FeatureAdded));
            FeatureRemoved = new(this, nameof(FeatureRemoved));
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

        private void RouteEvents(Floor floor)
        {
            floor.TileChanged += (f, oldTile, newTile) => TileChanged.Raise(new(floor, oldTile, newTile));
            floor.ActorAdded += (f, a) => ActorAdded.Raise(new(f, null, a));
            floor.ActorRemoved += (f, a) => ActorRemoved.Raise(new(f, a, null));
            floor.ItemAdded += (f, a) => ItemAdded.Raise(new(f, null, a));
            floor.ItemRemoved += (f, a) => ItemRemoved.Raise(new(f, a, null));
            floor.FeatureAdded += (f, a) => FeatureAdded.Raise(new(f, null, a));
            floor.FeatureRemoved += (f, a) => FeatureRemoved.Raise(new(f, a, null));
        }

        public void AddFloor(FloorId id, Coord size, Func<FloorBuilder, FloorBuilder> configure)
        {
            var builder = configure(ServiceProvider.GetInstance<FloorBuilder>());
            var floor = builder.Build(id, size);
            RouteEvents(floor);
            Floors.Add(id, floor);
        }

        public void AddDungeon(Func<DungeonBuilder, DungeonBuilder> configure)
        {
            var builder = configure(ServiceProvider.GetInstance<DungeonBuilder>());
            foreach(var floor in builder.Build()) {
                RouteEvents(floor);
                Floors.Add(floor.Id, floor);
            }
        }

        public bool TryGetCellAt(FloorId id, Coord pos, out MapCell cell)
        {
            cell = default;
            return TryGetFloor(id, out var floor) && floor.Cells.TryGetValue(pos, out cell);
        }
        public MapCell GetCellAt(FloorId id, Coord pos) => TryGetCellAt(id, pos, out var cell) ? cell : null;

        public IEnumerable<PhysicalEntity> GetDrawables(FloorId id) => TryGetFloor(id, out var floor)
            ? floor.GetDrawables()
            : Enumerable.Empty<PhysicalEntity>();
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
        public void SetTileAt(FloorId id, Coord pos, Tile tile)
        {
            if (TryGetFloor(id, out var floor)) {
                if (TryGetCellAt(id, pos, out var old)) {
                    Entities.FlagEntityForRemoval(old.Tile.Id);
                }
                tile.Physics.FloorId = id;
                tile.Physics.Position = pos;
                floor.SetTile(tile);
            }
        }

        public IEnumerable<MapCell> GetNeighborhood(FloorId id, Coord pos, int size = 3)
        {
            size /= 2;
            for (int x = -size; x < size; x++) {
                for (int y = -size; y < size; y++) {
                    if (TryGetCellAt(id, new(pos.X + x, pos.Y + y), out var n))
                        yield return n;
                }
            }
        }

        public bool TryGetClosestFreeTile(FloorId id, Coord pos, out Tile closest, float maxDistance = 10, Func<MapCell, bool> pred = null)
        {
            closest = default;
            pred ??= cell => !cell.Items.Any() && !cell.Features.Any(f => f.Physics.BlocksMovement) && !cell.Actors.Any();

            if (TryGetCellAt(id, pos, out var closestCell) && pred(closestCell)) {
                closest = closestCell.Tile;
                return true;
            }
            if (--maxDistance <= 0)
                return false;
            var neighbors = GetNeighborhood(id, pos)
                .Select(c => c.Tile)
                .Where(n => !n.Physics.BlocksMovement)
                .Shuffle(Rng.Random)
                .ToList();
            var maxDistanceSquared = maxDistance * maxDistance;
            if (neighbors.All(n => n.SquaredDistanceFrom(pos) > maxDistanceSquared))
                return false;
            foreach (var n in neighbors) {
                if (TryGetClosestFreeTile(id, n.Position(), out closest, maxDistance, pred)) {
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
        public bool RemoveActor(Actor actor)
        {
            if (TryGetFloor(actor.FloorId(), out var floor)) {
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
        public bool RemoveItem(Item item)
        {
            if (TryGetFloor(item.FloorId(), out var floor)) {
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
        public bool RemoveFeature(Feature feature)
        {
            if (TryGetFloor(feature.FloorId(), out var floor)) {
                floor.RemoveFeature(feature);
                return true;
            }
            return false;
        }

        public void RecalculateFov(Actor a, Coord? overridePosition = null)
        {
            var floorId = a.FloorId();
            if(a.Fov != null && TryGetFloor(floorId, out var floor)) {
                if (!a.Fov.KnownTiles.TryGetValue(floorId, out var knownTiles)) {
                    a.Fov.KnownTiles[floorId] = knownTiles = new();
                }
                if (!a.Fov.VisibleTiles.TryGetValue(floorId, out var visibleTiles)) {
                    a.Fov.VisibleTiles[floorId] = visibleTiles = new();
                }
                visibleTiles.Clear();
                visibleTiles.UnionWith(floor.CalculateFov(overridePosition ?? a.Position(), a.Fov.Radius));
                knownTiles.UnionWith(visibleTiles);
            }
        }

        public bool IsLineOfSightBlocked(FloorId id, Coord a, Coord b)
            => !TryGetFloor(id, out var floor) 
                || Shapes.Line(a, b).Any(p => !floor.Cells.TryGetValue(p, out var cell) || !cell.Tile.IsWalkable(null));
    }
}
