namespace Fiero.Business
{
    public class FloorGenerationContext(GameEntityBuilders builders, FloorId id, Coord size)
    {
        public readonly FloorId Id = id;
        public readonly Coord Size = size;

        public readonly LogComponent Log;
        public DungeonTheme Theme { get; set; } = DungeonTheme.Default;

        protected readonly GameEntityBuilders EntityBuilders = builders;
        protected readonly Dictionary<Coord, HashSet<ObjectDef>> Objects = new();
        protected readonly Dictionary<Coord, TileDef> Tiles = new();
        protected readonly HashSet<FloorConnection> Connections = new();
        protected readonly HashSet<Coord> SpawnPoints = new();


        public FloorGenerationContext CreateSubContext(Coord size)
        {
            return new(EntityBuilders, Id, size);
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
                throw new ArgumentOutOfRangeException($"{nameof(pos)}: {pos}/{Size}");
            Tiles[pos] = tile.WithPosition(pos);
        }

        public void AddObject<T>(string name, Coord pos, Func<GameEntityBuilders, IEntityBuilder<T>> build)
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
            list.Add(new(name, typeof(T), false, pos, null, id => build(EntityBuilders).WithPosition(pos, id).Build()));
        }

        public void AddMetaObject<T>(string name, Coord pos, T data)
        {
            if (pos.X < 0 || pos.Y < 0 || pos.X >= Size.X || pos.Y >= Size.Y)
                throw new ArgumentOutOfRangeException(nameof(pos));
            if (!Objects.TryGetValue(pos, out var list))
            {
                list = Objects[pos] = new();
            }
            list.Add(new(name, typeof(T), false, pos, data, null));
        }

        public bool TryAddFeature<T>(string name, IEnumerable<Coord> validPositions, Func<GameEntityBuilders, IEntityBuilder<T>> build, out Coord pos)
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

        public bool TryAddFeature<T>(string name, Coord pos, Func<GameEntityBuilders, IEntityBuilder<T>> build)
            where T : Feature
        {
            if (!IsPointInBounds(pos))
                throw new ArgumentOutOfRangeException(nameof(pos));
            if (!Objects.TryGetValue(pos, out var list))
            {
                list = Objects[pos] = new();
            }
            list.Add(new(name, typeof(T), true, pos, null, id => build(EntityBuilders).WithPosition(pos, id).Build()));
            return true;
        }

        public void AddConnections(params FloorConnection[] c) => Connections.UnionWith(c);

        public IEnumerable<ObjectDef> GetObjects() => Objects.Values.SelectMany(v => v);
        public int RemoveObjects(Func<ObjectDef, bool> pred)
        {
            var toRemove = new HashSet<Coord>();
            foreach (var (pos, list) in Objects)
            {
                list.RemoveWhere(o => pred(o));
                if (list.Count == 0)
                    toRemove.Add(pos);
            }

            var ret = toRemove.Count;
            foreach (var key in toRemove)
                Objects.Remove(key);
            return ret;
        }
        public void AddSpawnPoint(Coord c) => SpawnPoints.Add(c);
        public IEnumerable<Coord> GetSpawnPoints() => SpawnPoints;
        public IEnumerable<ObjectDef> GetObjectsAt(Coord p) => Objects.TryGetValue(p, out var set) ? set : Enumerable.Empty<ObjectDef>();
        public IEnumerable<TileDef> GetTiles() => Tiles.Values;
        public TileDef GetTile(Coord p) => Tiles[p];
        public bool TryGetTile(Coord p, out TileDef tile) => Tiles.TryGetValue(p, out tile);
        /// Returns all tiles without dungeon features
        public IEnumerable<TileDef> GetEmptyTiles() => Tiles
                .Where(x => !GetObjectsAt(x.Key).Where(x => x.IsFeature).Any())
                .Select(x => x.Value);
        public IEnumerable<FloorConnection> GetConnections() => Connections;
    }
}
