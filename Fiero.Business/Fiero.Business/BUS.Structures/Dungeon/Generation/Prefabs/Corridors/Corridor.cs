namespace Fiero.Business
{
    public class Corridor : ThemedFloorGenerationPrefab
    {
        public RoomConnector Start { get; private set; }
        public RoomConnector End { get; private set; }
        public int MiddleThickness { get; private set; } = 1;
        public int StartThickness { get; private set; } = 1;
        public int EndThickness { get; private set; } = 1;

        public int EffectiveStartThickness => HasMiddleJoint ? StartThickness : MiddleThickness;
        public int EffectiveEndThickness => HasMiddleJoint ? EndThickness : MiddleThickness;

        public Coord[] Points { get; private set; }

        public int Length { get; private set; }
        public bool HasMiddleJoint { get; private set; }

        public Corridor(RoomConnector a, RoomConnector b)
        {
            Start = a;
            End = b;
            UpdateGeometry();
        }

        protected void UpdateGeometry()
        {
            var (a, b) = (Start, End);
            var aEdge = new Line(a.Edge.Left, a.Edge.Right);
            var bEdge = new Line(b.Edge.Left, b.Edge.Right);
            var aPoints = Shapes.Line(aEdge.Start, aEdge.End).ToHashSet();
            var bPoints = Shapes.Line(bEdge.Start, bEdge.End).ToHashSet();
            var (aMidSide, bMidSide) = (aEdge.DeterminePointSide(b.Middle), bEdge.DeterminePointSide(a.Middle));
            Points = Generate()
                // Generates creates a thick corridor that may exceed the edge of the connectors
                // so we need to trim the excess by removing all points that fall on the wrong side
                .Where(p =>
                {
                    var (aPSide, bPSide) = (aEdge.DeterminePointSide(p), bEdge.DeterminePointSide(p));
                    if (aPSide != aMidSide || bPSide != bMidSide) return false;
                    // Also remove points falling directly on the connector, as we need to draw the door frame there
                    if (aPoints.Contains(p) || bPoints.Contains(p)) return false;
                    return true;
                })
                .ToArray();
        }

        protected override DungeonTheme CustomizeTheme(DungeonTheme theme)
        {
            theme = base.CustomizeTheme(theme);

            var roll = theme.CorridorThickness
                .Roll();
            if (theme.UnevenCorridors)
            {
                StartThickness = roll.Take(1).Single();
                EndThickness = roll.Take(1).Single();
                MiddleThickness = roll.Take(1).Single();
            }
            else
            {
                StartThickness = MiddleThickness = EndThickness = roll.Take(1).Single();
            }
            if (Start != null && End != null)
                UpdateGeometry();
            return theme;
        }

        IEnumerable<Coord> Generate()
        {
            Length = 0;
            var startMiddle = Start.Middle;
            var endMiddle = End.Middle;
            var v1 = (Start.Edge.Left - Start.Edge.Right).Clamp(-1, 1);
            var d1 = (int)(Math.Atan2(v1.Y, v1.X) * 180f / Math.PI);
            var v2 = (End.Edge.Left - End.Edge.Right).Clamp(-1, 1);
            var d2 = (int)(Math.Atan2(v2.Y, v2.X) * 180f / Math.PI);
            var connectStart = startMiddle;
            var connectEnd = endMiddle;
            var middle = (startMiddle + endMiddle) / 2;
            var l1 = new Line(startMiddle, startMiddle + new Coord(v1.Y, v1.X));
            var l2 = new Line(endMiddle, endMiddle + new Coord(v2.Y, v2.X));
            HasMiddleJoint = !((middle.X == startMiddle.X && middle.X == endMiddle.X)
                || (middle.Y == startMiddle.Y && middle.Y == endMiddle.Y));
            if (!l1.IsParallel(l2))
            {
                middle = new(
                    (l1.B * l2.C - l2.B * l1.C) / (l1.A * l2.B - l2.A * l1.B),
                    (l1.C * l2.A - l2.C * l1.A) / (l1.A * l2.B - l2.A * l1.B)
                );
            }
            switch (d1.Mod(360))
            {
                case 0:
                case 180:
                    connectStart = new(startMiddle.X, middle.Y);
                    Length += startMiddle.DistManhattan(connectStart);
                    foreach (var p in Shapes.ThickLine(startMiddle, connectStart, EffectiveStartThickness))
                        yield return p;
                    break;
                case 90:
                case 270:
                    connectStart = new(middle.X, startMiddle.Y);
                    Length += startMiddle.DistManhattan(connectStart);
                    foreach (var p in Shapes.ThickLine(startMiddle, connectStart, EffectiveStartThickness)) yield return p;
                    break;
            }
            switch (d2.Mod(360))
            {
                case 0:
                case 180:
                    connectEnd = new(endMiddle.X, middle.Y);
                    Length += endMiddle.DistManhattan(connectEnd);
                    foreach (var p in Shapes.ThickLine(endMiddle, connectEnd, EffectiveEndThickness)) yield return p;
                    break;
                case 90:
                case 270:
                    connectEnd = new(middle.X, endMiddle.Y);
                    Length += endMiddle.DistManhattan(connectEnd);
                    foreach (var p in Shapes.ThickLine(endMiddle, connectEnd, EffectiveEndThickness)) yield return p;
                    break;
            }
            if (HasMiddleJoint)
            {
                Length += connectStart.DistManhattan(connectEnd);
                foreach (var p in Shapes.ThickLine(connectStart, connectEnd, MiddleThickness))
                    yield return p;
            }
        }

        public virtual void DrawPoints(FloorGenerationContext ctx)
        {
            foreach (var p in Points)
                ctx.Draw(p, Theme.CorridorTile);
        }

        public virtual void DrawDoors(FloorGenerationContext ctx, bool start = true, bool end = true)
        {
            var startMiddle = (Start.Edge.Left + Start.Edge.Right) / 2;
            var endMiddle = (End.Edge.Left + End.Edge.Right) / 2;
            if (start
                && Start.Owner.Theme.DoorChance.Check() && !ctx.GetObjects().Any(obj => obj.Position == startMiddle)
                && (EffectiveStartThickness) % 2 == 1) // don't draw doors when the thickness is even: they're ugly
            {
                DrawDoorAndFrame(Start);
            }
            else if (start)
            {
                DrawOpenFrame(Start);
            }
            if (end
                && End.Owner.Theme.DoorChance.Check() && !ctx.GetObjects().Any(obj => obj.Position == endMiddle)
                && (EffectiveEndThickness) % 2 == 1) // TODO: Add double door for 2-wide?
            {
                DrawDoorAndFrame(End);
            }
            else if (end)
            {
                DrawOpenFrame(End);
            }

            void DrawOpenFrame(RoomConnector edge)
            {
                foreach (var p in Shapes.Line(edge.Edge.Left, edge.Edge.Right))
                {
                    if (!Points.Any(q => q.CardinallyAdjacent(p)))
                        continue;
                    ctx.SetTile(p, Theme.CorridorTile(p));
                }
            }

            void DrawDoorAndFrame(RoomConnector edge)
            {
                foreach (var p in Shapes.Line(edge.Edge.Left, edge.Edge.Right))
                {
                    if (!Points.Any(q => q.CardinallyAdjacent(p)))
                        continue;
                    ctx.SetTile(p, Theme.WallTile(p));
                    if (p == edge.Middle)
                    {
                        ctx.SetTile(p, Theme.CorridorTile(p));
                        ctx.TryAddFeature(nameof(FeatureName.Door), p, e => Theme.DoorFeature(e, startMiddle));
                    }
                }
            }
        }

        public override void Draw(FloorGenerationContext ctx)
        {
            DrawPoints(ctx);
            DrawDoors(ctx);
        }
    }
}
