using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Fiero.Core
{

    public static class TrigonometryExtensions
    {
        public static int Mod(this int x, int m) => (x % m + m) % m;
        public static double Dist(this Coord p, Coord q) => Math.Sqrt(Math.Pow(p.X - q.X, 2) + Math.Pow(p.Y - q.Y, 2));
        public static double DistSq(this Coord p, Coord q) => Math.Pow(p.X - q.X, 2) + Math.Pow(p.Y - q.Y, 2);
        public static Vec ToVec(this Vector2u v) => new(v.X, v.Y);
        public static Vec ToVec(this Vector2i v) => new(v.X, v.Y);
        public static Vec ToVec(this Vector2f v) => new(v.X, v.Y);
        public static Vec ToVec(this Point v) => new(v.X, v.Y);
        public static Vec ToVec(this Coord v) => new(v.X, v.Y);
        public static int Area(this Coord c) => c.X * c.Y;
        public static Coord ToCoord(this Vector2u v) => new((int)v.X, (int)v.Y);
        public static Coord ToCoord(this Vector2i v) => new(v.X, v.Y);
        public static Coord ToCoord(this Vector2f v) => new((int)v.X, (int)v.Y);
        public static Coord ToCoord(this Point v) => new(v.X, v.Y);
        public static Coord ToCoord(this Vec v) => new((int)v.X, (int)v.Y);
        public static Vec Rotate(this Vec v, float theta) => new(
            (float)(Math.Cos(theta * v.X) - Math.Sin(theta * v.Y)),
            (float)(Math.Sin(theta * v.X) + Math.Cos(theta * v.Y))
        );
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
            for (int x = 0; x < rect.Width; x++) {
                for (int y = 0; y < rect.Height; y++) {
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
            for (int i = 0; i < subdivisions.X; i++) {
                var xmod = rect.Width % cellSize.X;
                var x = xmod == 0
                    ? (int)cellSize.X
                    : (int)cellSize.X + (int)(xmod * (i % 2));
                var cumY = 0;
                for (int j = 0; j < subdivisions.Y; j++) {
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
