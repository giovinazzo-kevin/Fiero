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
        protected readonly TileDef[,] Tiles;
        protected readonly HashSet<ObjectDef> Objects;
        protected readonly HashSet<FloorConnection> Connections;

        public TileDef GetTile(Coord c) => Tiles[c.X, c.Y];
        public IEnumerable<TileDef> GetAllTiles() => Enumerable.Range(0, Size.X)
            .SelectMany(x => Enumerable.Range(0, Size.Y)
                .Select(y => Tiles[x, y]));
        public void SetTile(Coord c, TileName value, ColorName? color = null)
        {
            if(c.X >= 0 && c.Y >= 0 && c.X < Size.X && c.Y < Size.Y) {
                var tile = new TileDef(value, c, color);
                Tiles[c.X, c.Y] = tile;
            }
        }

        public void AddObject(DungeonObjectName obj, Coord pos) => Objects.Add(new ObjectDef(obj, pos));
        public void AddConnection(FloorConnection c) => Connections.Add(c);
        public void AddConnections(IEnumerable<FloorConnection> c) => Connections.UnionWith(c);
        public IEnumerable<ObjectDef> GetObjects() => Objects;
        public IEnumerable<FloorConnection> GetConnections() => Connections;

        public void ForEach(Action<TileDef> xy)
        {
            for (var y = 0; y < Size.Y; y++) {
                for (var x = 0; x < Size.X; x++) {
                    xy(Tiles[x, y]);
                }
            }
        }
        public FloorGenerationContext(int width, int height)
        {
            Size = new Coord(width, height);
            Tiles = new TileDef[width, height];
            Objects = new();
            Connections = new();
        }
    }
}
