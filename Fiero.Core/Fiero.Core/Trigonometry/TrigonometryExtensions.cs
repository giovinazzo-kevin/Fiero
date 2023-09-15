using Fiero.Core.Structures;
using SFML.Graphics;
using SFML.System;
using System.Drawing;

namespace Fiero.Core
{

    public static class TrigonometryExtensions
    {
        public static int DefaultIfZero(this int x, int d) => x == 0 ? d : x;
        public static int Mod(this int x, int m) => (x % m + m) % m;
        public static double Dist(this Coord p, Coord q) => Math.Sqrt(Math.Pow(p.X - q.X, 2) + Math.Pow(p.Y - q.Y, 2));
        public static int DistSq(this Coord p, Coord q) => (int)Math.Pow(p.X - q.X, 2) + (int)Math.Pow(p.Y - q.Y, 2);
        public static int DistManhattan(this Coord p, Coord q) => Math.Abs(p.X - q.X) + Math.Abs(p.Y - q.Y);
        public static int DistChebyshev(this Coord p, Coord q) => Math.Max(Math.Abs(p.X - q.X), Math.Abs(p.Y - q.Y));
        public static bool CardinallyAdjacent(this Coord p, Coord q) => p.DistSq(q) == 1;
        public static bool DiagonallyAdjacent(this Coord p, Coord q) => p.DistSq(q) == 2;
        public static Vec ToVec(this Vector2u v) => new(v.X, v.Y);
        public static Vec ToVec(this Vector2i v) => new(v.X, v.Y);
        public static Vec ToVec(this Vector2f v) => new(v.X, v.Y);
        public static Vec ToVec(this Point v) => new(v.X, v.Y);
        public static Vec ToVec(this Coord v) => new(v.X, v.Y);
        public static int Area(this Coord c) => c.X * c.Y;
        public static Coord ToCoord(this Vector2u v) => new((int)v.X, (int)v.Y);
        public static Coord ToCoord(this Vector2i v) => new(v.X, v.Y);
        public static Coord ToCoord(this Vector2f v) => new((int)v.X, (int)v.Y);
        public static Vector2f Round(this Vector2f v, int digits = 0) => new((float)Math.Round(v.X, digits), (float)Math.Round(v.Y, digits));
        public static Vec Round(this Vec v, int digits = 0) => new((float)Math.Round(v.X, digits), (float)Math.Round(v.Y, digits));
        public static Coord ToCoord(this Point v) => new(v.X, v.Y);
        public static Coord ToCoord(this Vec v) => new((int)v.X, (int)v.Y);
        public static Vec RotateAround(this Vec v, Vec pivot, float theta)
        {
            var cT = (float)Math.Cos(theta);
            var sT = (float)Math.Sin(theta);
            return new(
                cT * (v.X - pivot.X) - sT * (v.Y - pivot.Y) + pivot.X,
                sT * (v.X - pivot.X) + cT * (v.Y - pivot.Y) + pivot.Y
            );
        }
        public static Vector2f ToVector2f(this Vec v) => new(v.X, v.Y);
        public static Vector2i ToVector2i(this Vec v) => new((int)v.X, (int)v.Y);
        public static Vector2u ToVector2u(this Vec v) => new((uint)v.X, (uint)v.Y);
        public static Point ToPoint(this Vec v) => new((int)v.X, (int)v.Y);
        public static Vector2f ToVector2f(this Coord v) => new(v.X, v.Y);
        public static Vector2i ToVector2i(this Coord v) => new(v.X, v.Y);
        public static Vector2u ToVector2u(this Coord v) => new((uint)v.X, (uint)v.Y);
        public static Point ToPoint(this Coord v) => new(v.X, v.Y);
        public static IntRect ToRect(this Coord v) => new(0, 0, v.X, v.Y);
        public static Coord Size(this IntRect rect) => new(rect.Width, rect.Height);
        public static Coord Position(this IntRect rect) => new(rect.Left, rect.Top);
        public static Coord Center(this IntRect rect) => rect.Position() + rect.Size() / 2;
        public static Vec Size(this FloatRect rect) => new(rect.Width, rect.Height);
        public static Vec Position(this FloatRect rect) => new(rect.Left, rect.Top);
        public static Coord Align(this Coord c, Coord to) => new(c.X - (c.X % to.X), c.Y - (c.Y % to.Y));
        public static IEnumerable<Coord> Enumerate(this IntRect rect)
        {
            for (int x = 0; x < rect.Width; x++)
            {
                for (int y = 0; y < rect.Height; y++)
                {
                    yield return new(rect.Left + x, rect.Top + y);
                }
            }
        }
        public static IEnumerable<UnorderedPair<Coord>> GetEdges(this IntRect rect)
        {
            yield return new(new(rect.Left, rect.Top), new(rect.Left + rect.Width, rect.Top));
            yield return new(new(rect.Left + rect.Width, rect.Top), new(rect.Left + rect.Width, rect.Top + rect.Height));
            yield return new(new(rect.Left + rect.Width, rect.Top + rect.Height), new(rect.Left, rect.Top + rect.Height));
            yield return new(new(rect.Left, rect.Top + rect.Height), new(rect.Left, rect.Top));
        }
        public static IEnumerable<IntRect> Subdivide(this IntRect rect, Coord subdivisions)
        {
            var cellSize = rect.Size() / subdivisions.ToVec();
            var cumX = 0;
            for (int i = 0; i < subdivisions.X; i++)
            {
                var xmod = rect.Width % cellSize.X;
                var x = xmod == 0
                    ? (int)cellSize.X
                    : (int)cellSize.X + (int)(xmod * (i % 2));
                var cumY = 0;
                for (int j = 0; j < subdivisions.Y; j++)
                {
                    var ymod = rect.Height % cellSize.Y;
                    var y = ymod == 0
                        ? (int)cellSize.Y
                        : (int)cellSize.X + (int)(ymod * (j % 2));
                    yield return new(rect.Left + cumX, rect.Top + cumY, x, y);
                    cumY += y;
                }
                cumX += x;
            }
        }
    }
}
