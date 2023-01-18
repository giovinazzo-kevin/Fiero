using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class FloorGenerationContext
    {
        public readonly Coord Size;

        public readonly LogComponent Log;
        protected readonly GameEntityBuilders EntityBuilders;
        protected readonly Dictionary<Coord, HashSet<ObjectDef>> Objects;
        protected readonly Dictionary<Coord, TileDef> Tiles;
        protected readonly HashSet<FloorConnection> Connections;


        public FloorGenerationContext(GameEntityBuilders builders, Coord size)
        {
            Size = size;
            EntityBuilders = builders;
            Objects = new();
            Tiles = new();
            Connections = new();
        }

        public bool IsPointInBounds(Coord pos)
        {
            if (pos.X < 0 || pos.Y < 0 || pos.X >= Size.X || pos.Y >= Size.Y)
                return false;
            return true;
        }

        public void SetTile(Coord pos, TileDef tile)
        {
            if (!IsPointInBounds(pos))
                throw new ArgumentOutOfRangeException(nameof(pos));
            Tiles[pos] = tile.WithPosition(pos);
        }

        public void AddObject<T>(string name, Coord pos, Func<GameEntityBuilders, EntityBuilder<T>> build)
            where T : PhysicalEntity
        {
            if (typeof(T).IsAssignableFrom(typeof(Feature)))
                throw new InvalidOperationException("Use TryAddFeature<T> to add dungeon features");
            if (pos.X < 0 || pos.Y < 0 || pos.X >= Size.X || pos.Y >= Size.Y)
                throw new ArgumentOutOfRangeException(nameof(pos));
            if (!Objects.TryGetValue(pos, out var list))
            {
                list = Objects[pos] = new();
            }
            list.Add(new(name, false, pos, id => build(EntityBuilders).WithPosition(pos, id).Build()));
        }

        public bool TryAddFeature<T>(string name, IEnumerable<Coord> validPositions, Func<GameEntityBuilders, EntityBuilder<T>> build, out Coord pos)
            where T : Feature
        {
            foreach (var item in validPositions)
            {
                if (TryAddFeature(name, pos = item, build))
                    return true;
            }
            pos = default;
            return false;
        }

        public bool TryAddFeature<T>(string name, Coord pos, Func<GameEntityBuilders, EntityBuilder<T>> build)
            where T : Feature
        {
            if (!IsPointInBounds(pos))
                throw new ArgumentOutOfRangeException(nameof(pos));
            if (!Objects.TryGetValue(pos, out var list))
            {
                list = Objects[pos] = new();
            }
            if (list.Any(x => x.IsFeature))
                return false;
            list.Add(new(name, true, pos, id => build(EntityBuilders).WithPosition(pos, id).Build()));
            return true;
        }

        public void AddConnections(params FloorConnection[] c) => Connections.UnionWith(c);

        public IEnumerable<ObjectDef> GetObjects() => Objects.Values.SelectMany(v => v);
        public IEnumerable<ObjectDef> GetObjectsAt(Coord p) => Objects.TryGetValue(p, out var set) ? set : Enumerable.Empty<ObjectDef>();
        public IEnumerable<TileDef> GetTiles() => Tiles.Values;
        public TileDef GetTile(Coord p) => Tiles[p];
        /// Returns all tiles without dungeon features
        public IEnumerable<TileDef> GetEmptyTiles() => Tiles
                .Where(x => !GetObjectsAt(x.Key).Where(x => x.IsFeature).Any())
                .Select(x => x.Value);
        public IEnumerable<FloorConnection> GetConnections() => Connections;

    }
}
