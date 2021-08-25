using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;

namespace Fiero.Business
{
    public class FloorGenerationContext
    {
        public readonly Coord Size;

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

        public void SetTile(Coord pos, TileDef tile)
        {
            if (pos.X < 0 || pos.Y < 0 || pos.X >= Size.X || pos.Y >= Size.Y)
                throw new ArgumentOutOfRangeException(nameof(pos));
            Tiles[pos] = tile.WithPosition(pos);
        }

        public void AddObject<T>(string name, Coord pos, Func<GameEntityBuilders, EntityBuilder<T>> build)
            where T : PhysicalEntity
        {
            if (pos.X < 0 || pos.Y < 0 || pos.X >= Size.X || pos.Y >= Size.Y)
                throw new ArgumentOutOfRangeException(nameof(pos));
            if (!Objects.TryGetValue(pos, out var list)) {
                list = Objects[pos] = new();
            }
            list.Add(new(name, pos, id => build(EntityBuilders).WithPosition(pos, id).Build()));
        }

        public void AddConnections(params FloorConnection[] c) => Connections.UnionWith(c);

        public IEnumerable<ObjectDef> GetObjects() => Objects.Values.SelectMany(v => v);
        public IEnumerable<TileDef> GetTiles() => Tiles.Values;
        public TileDef GetTile(Coord p) => Tiles[p];
        public TileDef GetRandomTile(Func<TileDef, bool> match) => Tiles.Values.Shuffle(Rng.Random).First(match);
        public IEnumerable<FloorConnection> GetConnections() => Connections;

    }
}
