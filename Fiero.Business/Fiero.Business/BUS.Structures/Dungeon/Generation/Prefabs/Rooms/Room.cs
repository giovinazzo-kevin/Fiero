using Fiero.Core;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{

    public abstract class Room : IFloorGenerationPrefab
    {
        protected readonly HashSet<IntRect> Rects;
        protected readonly HashSet<int> Indices;
        protected readonly List<RoomConnector> Connectors;

        public event Action<Room, FloorGenerationContext> Drawn;

        public Coord Position { get; private set; }
        public Coord Size { get; private set; }

        public bool AllowMonsters { get; protected set; } = true;
        public bool AllowItems { get; protected set; } = true;
        public bool AllowFeatures { get; protected set; } = true;

        protected virtual TileDef WallTile(Coord c) => new(TileName.Wall, c);
        protected virtual TileDef GroundTile(Coord c) => new(TileName.Room, c);

        public IEnumerable<IntRect> GetRects() => Rects;
        public IEnumerable<int> GetIndices() => Indices;
        public IEnumerable<Coord> GetPointCloud() => Rects.SelectMany(r => r.Enumerate());

        public Room()
        {
            Rects = new();
            Indices = new();
            Connectors = new();
        }

        public void AddRect(int index, IntRect rect)
        {
            Indices.Add(index);
            Rects.Add(rect);
            Position = Rects.Min(r => r.Position());
            Size = Rects.Max(r => r.Position() + r.Size() - Position);
            Connectors.Clear();
            Connectors.AddRange(SelectConnectors());
        }

        protected virtual IEnumerable<RoomConnector> SelectConnectors()
        {
            return Rects.SelectMany(r => r.GetEdges())
                .ToLookup(r => r)
                .Where(l => l.Count() == 1)
                .SelectMany(l => l)
                .Select(l => new RoomConnector(this, l));
        }

        public IEnumerable<RoomConnector> GetConnectors() => Connectors;

        public virtual void Draw(FloorGenerationContext ctx)
        {
            foreach (var rect in Rects)
            {
                ctx.FillBox(rect.Position(), rect.Size(), GroundTile);
            }
            foreach (var conn in SelectConnectors())
            {
                ctx.DrawLine(conn.Edge.Left, conn.Edge.Right, WallTile);
            }
            Drawn?.Invoke(this, ctx);
        }
    }
}
