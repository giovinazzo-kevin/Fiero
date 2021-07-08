using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using System;
using System.Drawing;

namespace Fiero.Core
{

    public static class TrigonometryExtensions
    {
        public static double Dist(this Coord p, Coord q) => Math.Sqrt(Math.Pow(p.X - q.X, 2) + Math.Pow(p.Y - q.Y, 2));
        public static double DistSq(this Coord p, Coord q) => Math.Pow(p.X - q.X, 2) + Math.Pow(p.Y - q.Y, 2);
        public static Vec ToVec(this Vector2u v) => new(v.X, v.Y);
        public static Vec ToVec(this Vector2i v) => new(v.X, v.Y);
        public static Vec ToVec(this Vector2f v) => new(v.X, v.Y);
        public static Vec ToVec(this Point v) => new(v.X, v.Y);
        public static Vec ToVec(this Coord v) => new(v.X, v.Y);
        public static Coord ToCoord(this Vector2u v) => new((int)v.X, (int)v.Y);
        public static Coord ToCoord(this Vector2i v) => new(v.X, v.Y);
        public static Coord ToCoord(this Vector2f v) => new((int)v.X, (int)v.Y);
        public static Coord ToCoord(this Point v) => new(v.X, v.Y);
        public static Coord ToCoord(this Vec v) => new((int)v.X, (int)v.Y);
        public static Vector2f ToVector2f(this Vec v) => new(v.X, v.Y);
        public static Vector2i ToVector2i(this Vec v) => new((int)v.X, (int)v.Y);
        public static Vector2u ToVector2u(this Vec v) => new((uint)v.X, (uint)v.Y);
        public static Point ToPoint(this Vec v) => new((int)v.X, (int)v.Y);
        public static Vector2f ToVector2f(this Coord v) => new(v.X, v.Y);
        public static Vector2i ToVector2i(this Coord v) => new(v.X, v.Y);
        public static Vector2u ToVector2u(this Coord v) => new((uint)v.X, (uint)v.Y);
        public static Point ToPoint(this Coord v) => new(v.X, v.Y);
        public static Coord Size(this IntRect rect) => new(rect.Width, rect.Height);
        public static Coord Position(this IntRect rect) => new(rect.Left, rect.Top);
        public static Vec Size(this FloatRect rect) => new(rect.Width, rect.Height);
        public static Vec Position(this FloatRect rect) => new(rect.Left, rect.Top);
        public static Coord Align(this Coord c, Coord to) => new(c.X - (c.X % to.X), c.Y - (c.Y % to.Y));

    }
}
