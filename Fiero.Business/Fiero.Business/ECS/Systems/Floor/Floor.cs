using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Fiero.Business
{
    public class Floor
    {
        public readonly FloorId Id;
        public readonly Coord Size;

        private readonly SpatialDictionary<MapCell> _cells;
        public IReadOnlyDictionary<Coord, MapCell> Cells => _cells;
        public SpatialAStar<MapCell, object> Pathfinder { get; private set; }

        public Floor(FloorId id, FloorGenerationContext ctx)
        {
            Id = id;
            Size = ctx.Size;
            _cells = new SpatialDictionary<MapCell>(ctx.Size);
        }

        public IEnumerable<Drawable> GetDrawables() 
            => Cells.Values.SelectMany(c => c.GetDrawables());

        public void SetTile(Tile tile)
        {
            if (!_cells.TryGetValue(tile.Physics.Position, out var cell)) {
                cell = _cells[tile.Physics.Position] = new(tile);
            }
            cell.Tile = tile;
            if (Pathfinder != null) {
                Pathfinder.Update(tile.Physics.Position, cell, out var old);
                old?.Tile?.TryRefresh(tile.Id); // Update old references that are stored in pathfinding lists
            }
        }

        public void AddActor(Actor actor)
        {
            actor.ActorProperties.FloorId = Id;
            if(_cells.TryGetValue(actor.Physics.Position, out var cell)) {
                cell.Actors.Add(actor);
            }
        }

        public void RemoveActor(Actor actor)
        {
            if (_cells.TryGetValue(actor.Physics.Position, out var cell)) {
                cell.Actors.Remove(actor);
            }
        }

        public void AddItem(Item item)
        {
            if (_cells.TryGetValue(item.Physics.Position, out var cell)) {
                cell.Items.Add(item);
            }
        }

        public void RemoveItem(Item item)
        {
            if (_cells.TryGetValue(item.Physics.Position, out var cell)) {
                cell.Items.Remove(item);
            }
        }

        public void AddFeature(Feature feature)
        {
            feature.FeatureProperties.FloorId = Id;
            if (_cells.TryGetValue(feature.Physics.Position, out var cell)) {
                cell.Features.Add(feature);
            }
        }

        public void RemoveFeature(Feature feature)
        {
            if (_cells.TryGetValue(feature.Physics.Position, out var cell)) {
                cell.Features.Remove(feature);
            }
        }

        public void CreatePathfinder()
        {
            Pathfinder = _cells.GetPathfinder();
            foreach (var feature in Cells.Values.SelectMany(c => c.Features).Where(f => f.FeatureProperties.BlocksMovement)) {
                Pathfinder.Update(feature.Physics.Position, null, out _);
            }
        }
    }
}
