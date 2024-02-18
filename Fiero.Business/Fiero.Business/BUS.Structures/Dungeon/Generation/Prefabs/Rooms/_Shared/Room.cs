using SFML.Graphics;

namespace Fiero.Business
{

    public abstract class Room : ThemedFloorGenerationPrefab
    {
        protected readonly HashSet<IntRect> Rects;
        protected readonly HashSet<int> Indices;
        protected readonly List<RoomConnector> Connectors;

        public event Action<Room, FloorGenerationContext> Drawn;

        public Coord Position { get; private set; }
        public Coord Size { get; private set; }

        public RoomTree Tree { get; internal set; }
        public bool AllowMonsters { get; protected set; } = true;
        public bool AllowItems { get; protected set; } = true;
        public bool AllowFeatures { get; protected set; } = true;

        public bool Disconnected => !Connectors.Any(x => x.IsUsed);

        public IEnumerable<IntRect> GetRects() => Rects;
        public IEnumerable<int> GetIndices() => Indices;
        public IEnumerable<Coord> GetPointCloud() => Rects.SelectMany(r => r.Enumerate());

        public Room()
        {
            Rects = new();
            Indices = new();
            Connectors = new();
        }

        public void AddRect(RoomRect rect)
        {
            Indices.Add(rect.GridIndex);
            Rects.Add(rect.Rect);
            Position = Rects.Min(r => r.Position());
            Size = Rects.Max(r => r.Position() + r.Size() - Position);
            Connectors.Clear();
            Connectors.AddRange(Rects.SelectMany(r => r.GetEdges())
                .ToLookup(r => r)
                .Where(l => l.Count() == 1)
                .SelectMany(l => l)
                .Select(l => new RoomConnector(this, l)));
        }

        public IEnumerable<RoomConnector> GetConnectors() => Connectors;

        public override void Draw(FloorGenerationContext ctx)
        {
            foreach (var rect in Rects)
            {
                ctx.FillBox(rect.Position(), rect.Size(), Theme.RoomTile);
            }
            foreach (var conn in Connectors)
            {
                if (conn.IsHidden)
                    continue;

                ctx.DrawLine(conn.Edge.Left, conn.Edge.Right, Theme.WallTile);
            }
            OnDrawn(ctx);
        }

        protected void OnDrawn(FloorGenerationContext ctx)
        {
            Drawn?.Invoke(this, ctx);
        }
    }
}
