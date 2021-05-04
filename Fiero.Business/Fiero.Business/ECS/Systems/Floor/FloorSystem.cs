using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Fiero.Business
{
    public class FloorSystem
    {
        protected readonly List<Floor> Map;
        protected readonly GameEntities Entities;
        protected readonly GameDataStore Store;

        public Floor CurrentFloor => Map.FirstOrDefault();
        public IEnumerable<Drawable> GetDrawables() => CurrentFloor.GetDrawables();

        public FloorSystem(GameEntities entities, GameDataStore store)
        {
            Entities = entities;
            Store = store;
            Map = new List<Floor>();
        }

        public void Clear()
        {
            foreach (var floor in Map) {
                foreach (var tile in floor.Tiles.Values) {
                    Entities.FlagEntityForRemoval(tile.Id);
                }
                foreach (var actor in floor.Actors) {
                    Entities.FlagEntityForRemoval(actor.Id);
                }
            }
            Entities.RemoveFlaggedItems(propagate: true);
            Map.Clear();
        }

        public bool TileAt(Coord pos, out Tile tile) => CurrentFloor.Tiles.TryGetValue(pos, out tile);
        public IEnumerable<Actor> ActorsAt(Coord pos) => CurrentFloor.Actors.Where(a => a.Physics.Position == pos);
        public IEnumerable<Item> ItemsAt(Coord pos) => CurrentFloor.Items.Where(a => a.Physics.Position == pos);
        public IEnumerable<Feature> FeaturesAt(Coord pos) => CurrentFloor.Features.Where(a => a.Physics.Position == pos);

        public bool TryGetClosestFreeTile(Coord pos, out Tile closest, float maxDistance = 10)
        {
            if (TileAt(pos, out closest)
                && !ActorsAt(pos).Any() 
                && !FeaturesAt(pos).Any(f => f.Properties.BlocksMovement)) {
                return true;
            }
            if (--maxDistance <= 0)
                return false;
            var neighbors = GetNeighbors(pos)
                .Where(n => !n.Properties.BlocksMovement)
                .OrderBy(n => Rng.Random.NextDouble())
                .ToList();
            if(neighbors.All(n => n.DistanceFrom(pos) > maxDistance))
                return false;
            foreach (var n in neighbors) {
                if (TryGetClosestFreeTile(n.Physics.Position, out closest, maxDistance)) {
                    return true;
                }
            }
            return false;
        }

        public IEnumerable<Tile> GetNeighbors(Coord pos)
        {
            if (TileAt(new(pos.X - 1, pos.Y - 1), out var n1))
                yield return n1;
            if (TileAt(new(pos.X, pos.Y - 1), out var n2))
                yield return n2;
            if (TileAt(new(pos.X + 1, pos.Y - 1), out var n3))
                yield return n3;
            if (TileAt(new(pos.X - 1, pos.Y), out var n4))
                yield return n4;
            if (TileAt(new(pos.X, pos.Y), out var n5))
                yield return n5;
            if (TileAt(new(pos.X + 1, pos.Y), out var n6))
                yield return n6;
            if (TileAt(new(pos.X - 1, pos.Y + 1), out var n7))
                yield return n7;
            if (TileAt(new(pos.X, pos.Y + 1), out var n8))
                yield return n8;
            if (TileAt(new(pos.X + 1, pos.Y + 1), out var n9))
                yield return n9;
        }

        public void UpdateTile(Coord pos, TileName tile)
        {
            if(TileAt(pos, out var old)) {
                CurrentFloor.Entities.FlagEntityForRemoval(old.Id);
            }
            var id = CurrentFloor.Entities.CreateTile(tile, pos);
            CurrentFloor.SetTile(id);
        }


        public void AddFloor(Coord size, Func<FloorBuilder, FloorBuilder> configure)
        {
            var tileSize = Store.GetOrDefault(Data.UI.TileSize, 8);

            Map.Add(configure(new FloorBuilder(size, new(tileSize, tileSize))).Build(Entities.CreateScope()));
        }
    }
}
