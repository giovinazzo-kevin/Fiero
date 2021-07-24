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
        public class Object
        {
            public readonly DungeonObjectName Name;
            public readonly Coord Position;
            public Object(DungeonObjectName name, Coord pos) => (Name, Position) = (name, pos);
        }

        public readonly struct Tile
        {
            public readonly TileName Name;
            public readonly Coord Position;
            public Tile(TileName name, Coord pos) => (Name, Position) = (name, pos);
        }

        public readonly Coord Size;
        protected readonly TileName[,] Tiles;
        protected readonly HashSet<Object> Objects;
        protected readonly HashSet<FloorConnection> Connections;

        public TileName GetTile(Coord c) => Tiles[c.X, c.Y];
        public IEnumerable<Tile> GetAllTiles() => Enumerable.Range(0, Size.X)
            .SelectMany(x => Enumerable.Range(0, Size.Y)
                .Select(y => new Tile(Tiles[x, y], new Coord(x, y))));
        public void SetTile(Coord c, TileName value)
        {
            if(c.X >= 0 && c.Y >= 0 && c.X < Size.X && c.Y < Size.Y) {
                Tiles[c.X, c.Y] = value;
            }
        }

        public void AddObject(DungeonObjectName obj, Coord pos) => Objects.Add(new Object(obj, pos));
        public void AddConnection(FloorConnection c) => Connections.Add(c);
        public void AddConnections(IEnumerable<FloorConnection> c) => Connections.UnionWith(c);
        public IEnumerable<Object> GetObjects() => Objects;
        public IEnumerable<FloorConnection> GetConnections() => Connections;

        public void ForEach(Action<Tile> xy)
        {
            for (var y = 0; y < Size.Y; y++) {
                for (var x = 0; x < Size.X; x++) {
                    xy(new(Tiles[x, y], new Coord(x, y)));
                }
            }
        }

        public FloorGenerationContext(int width, int height)
        {
            Size = new Coord(width, height);
            Tiles = new TileName[width, height];
            Objects = new();
            Connections = new();
        }
    }
}
