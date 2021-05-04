using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
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

        public readonly Coord Size;
        protected readonly TileName[,] Tiles;
        protected readonly HashSet<Object> Objects;

        public TileName Get(int x, int y) => Tiles[x, y];
        public void Set(int x, int y, TileName value) => Tiles[x, y] = value;

        public void Add(DungeonObjectName obj, Coord pos) => Objects.Add(new Object(obj, pos));
        public IEnumerable<Object> GetObjects() => Objects;

        public void ForEach(Action<(Coord P, TileName Tile)> xy)
        {
            for (var y = 0; y < Size.Y; y++) {
                for (var x = 0; x < Size.X; x++) {
                    xy((new Coord(x, y), Tiles[x, y]));
                }
            }
        }

        public FloorGenerationContext(int width, int height)
        {
            Size = new Coord(width, height);
            Tiles = new TileName[width, height];
            Objects = new HashSet<Object>();
        }
    }
}
