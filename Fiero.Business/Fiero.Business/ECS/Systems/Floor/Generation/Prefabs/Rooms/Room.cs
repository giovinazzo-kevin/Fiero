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

        public event Action<Room, FloorGenerationContext> Drawn;

        public Coord Position { get; private set; }
        public Coord Size { get; private set; }

        protected virtual TileDef WallTile(Coord c) => new(TileName.Wall, c);
        protected virtual TileDef GroundTile(Coord c) => new(TileName.Ground, c);

        public IEnumerable<IntRect> GetRects() => Rects;
        public IEnumerable<Coord> GetPointCloud() => Rects.SelectMany(r => r.Enumerate());

        public Room()
        {
            Rects = new();
        }

        public void AddRect(IntRect rect)
        {
            Rects.Add(rect);
            Position = Rects.Min(r => r.Position());
            Size = Rects.Max(r => r.Position() + r.Size() - Position);
        }

        public virtual IEnumerable<UnorderedPair<Coord>> GetConnectors()
        {
            var openEdges = Rects.SelectMany(r => r.GetEdges())
                .ToLookup(r => r)
                .Where(l => l.Count() == 1)
                .SelectMany(l => l);
            foreach (var edge in openEdges) {
                yield return edge;
            }
        }

        public virtual void Draw(FloorGenerationContext ctx) {
            foreach (var rect in Rects) {
                ctx.FillBox(rect.Position(), rect.Size(), GroundTile);
            }
            foreach (var edge in GetConnectors()) {
                ctx.DrawLine(edge.Left, edge.Right, WallTile);
            }
            Drawn?.Invoke(this, ctx);
        }
    }
}
