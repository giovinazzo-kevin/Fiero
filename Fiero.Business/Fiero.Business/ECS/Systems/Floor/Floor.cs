using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Fiero.Business
{
    public class Floor
    {
        public readonly GameEntities Entities;

        private readonly SpatialDictionary<Tile> _tiles;
        private readonly HashSet<Actor> _actors;
        private readonly HashSet<Item> _items;
        private readonly HashSet<Feature> _features;

        public IReadOnlyDictionary<Coord, Tile> Tiles => _tiles;
        public IReadOnlyCollection<Actor> Actors => _actors;
        public IReadOnlyCollection<Item> Items => _items;
        public IReadOnlyCollection<Feature> Features => _features;

        public IEnumerable<Drawable> GetDrawables() => Tiles.Values.Cast<Drawable>()
            .Concat(Items)
            .Concat(Features)
            .Concat(Actors);

        public SpatialAStar<Tile, object> Pathfinder { get; private set; }

        public Tile SetTile(int entityId)
        {
            if (!Entities.TryGetProxy<Tile>(entityId, out var tile)) {
                throw new ArgumentException();
            }
            _tiles[tile.Physics.Position] = tile;
            if (Pathfinder != null) {
                Pathfinder.Update(tile.Physics.Position, tile, out var old);
                old?.TryRefresh(tile.Id); // Update old references that are stored in pathfinding lists
            }
            return tile;
        }

        public Actor AddActor(int entityId)
        {
            if (!Entities.TryGetProxy<Actor>(entityId, out var actor)) {
                throw new ArgumentException();
            }
            actor.ActorProperties.CurrentFloor = this;
            _actors.Add(actor);
            return actor;
        }
        public void RemoveActor(int entityId)
        {
            _actors.Remove(_actors.Single(v => v.Id == entityId));
        }
        public Item AddItem(int entityId)
        {
            if (!Entities.TryGetProxy<Item>(entityId, out var item)) {
                throw new ArgumentException();
            }
            _items.Add(item);
            return item;
        }
        public void RemoveItem(int entityId)
        {
            _items.Remove(_items.Single(v => v.Id == entityId));
        }
        public Feature AddFeature(int entityId)
        {
            if (!Entities.TryGetProxy<Feature>(entityId, out var feature)) {
                throw new ArgumentException();
            }
            _features.Add(feature);
            return feature;
        }
        public void RemoveFeature(int entityId)
        {
            _features.Remove(_features.Single(v => v.Id == entityId));
        }

        public Floor(GameEntities entities, FloorGenerationContext ctx)
        {
            Entities = entities;
            _tiles = new SpatialDictionary<Tile>(ctx.Size);
            _actors = new HashSet<Actor>();
            _items = new HashSet<Item>();
            _features = new HashSet<Feature>();
        }

        public void CreatePathfinder()
        {
            Pathfinder = _tiles.GetPathfinder();
            foreach (var feature in Features.Where(f => f.Properties.BlocksMovement)) {
                Pathfinder.Update(feature.Physics.Position, null, out _);
            }
        }
    }
}
