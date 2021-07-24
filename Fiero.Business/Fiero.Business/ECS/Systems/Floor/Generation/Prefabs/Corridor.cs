using Fiero.Core;
using System;
using System.Drawing;
using System.Linq;

namespace Fiero.Business
{
    class Line
    {
        public readonly int A, B, C;

        public Line(Coord a, Coord b)
        {
            A = (a.Y - b.Y);
            B = (b.X - a.X);
            C = (a.X * b.Y - b.X * a.Y);
        }

        public bool IsParallel(Line other) => A * other.B - B * other.A == 0;

        public bool Intersection(Line other, out Coord p)
        {
            var D = A * other.B - B * other.A;
            var Dx = C * other.B - B * other.C;
            var Dy = A * other.C - C * other.A;
            if (D != 0) {
                p = new(Dx / D, Dy / D);
                return true;
            }
            p = default;
            return false;
        }
    }

    public class Corridor : IFloorGenerationPrefab
    {

        public UnorderedPair<Coord> Start { get; set; }
        public UnorderedPair<Coord> End { get; set; }
        public int Thickness { get; set; }

        public TileName WallTile { get; set; } = TileName.Wall;
        public TileName GroundTile { get; set; } = TileName.Ground;

        public Corridor(UnorderedPair<Coord> a, UnorderedPair<Coord> b, int thickness = 1)
        {
            Start = a;
            End = b;
            Thickness = thickness;
        }

        public virtual void Draw(FloorGenerationContext ctx)
        {
            var startMiddle = (Start.Left + Start.Right) / 2;
            var endMiddle = (End.Left + End.Right) / 2;
            var v1 = (Start.Left - Start.Right).Clamp(-1, 1);
            var d1 = (int)(Math.Atan2(v1.Y, v1.X) * 180f / Math.PI);
            var v2 = (End.Left - End.Right).Clamp(-1, 1);
            var d2 = (int)(Math.Atan2(v2.Y, v2.X) * 180f / Math.PI);
            var connectStart = startMiddle;
            var connectEnd = endMiddle;
            var middle = (startMiddle + endMiddle) / 2;
            switch (d1.Mod(360)) {
                case 0:
                case 180:
                    ctx.DrawLine(startMiddle, connectStart = new(startMiddle.X, middle.Y), GroundTile);
                    break;
                case 90:
                case 270:
                    ctx.DrawLine(startMiddle, connectStart = new(middle.X, startMiddle.Y), GroundTile);
                    break;
            }
            switch (d2.Mod(360)) {
                case 0:
                case 180:
                    ctx.DrawLine(endMiddle, connectEnd = new(endMiddle.X, middle.Y), GroundTile);
                    break;
                case 90:
                case 270:
                    ctx.DrawLine(endMiddle, connectEnd = new(middle.X, endMiddle.Y), GroundTile);
                    break;
            }
            ctx.DrawLine(connectStart, connectEnd, GroundTile);
            if (Rng.Random.OneChanceIn(3) && !ctx.GetObjects().Any(obj => obj.Position == startMiddle)) {
                ctx.AddObject(DungeonObjectName.Door, startMiddle);
            }
            if (Rng.Random.OneChanceIn(3) && !ctx.GetObjects().Any(obj => obj.Position == endMiddle)) {
                ctx.AddObject(DungeonObjectName.Door, endMiddle);
            }
        }
    }
}
