using Fiero.Core;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public static class Utils
    {
        public static bool Bresenham(Coord start, Coord end, Func<Coord, bool> callback)
        {
            var dx = Math.Abs(end.X - start.X); var sx = start.X < end.X ? 1 : -1;
            var dy = Math.Abs(end.Y - start.Y); var sy = start.Y < end.Y ? 1 : -1;
            var er = (dx > dy ? dx : -dy) / 2;
            for (; ; ) {
                if (!callback(new(start.X, start.Y)))
                    return false;
                if (start.X == end.X && start.Y == end.Y)
                    break;
                var e2 = er;
                if (e2 > -dx) { er -= dy; start = new Coord(start.X + sx, start.Y); }
                if (e2 < dy) { er += dx; start = new Coord(start.X, start.Y + sy); }
            }
            return true;
        }

        public static IEnumerable<Coord> BresenhamPoints(Coord start, Coord end)
        {
            var dx = Math.Abs(end.X - start.X); var sx = start.X < end.X ? 1 : -1;
            var dy = Math.Abs(end.Y - start.Y); var sy = start.Y < end.Y ? 1 : -1;
            var er = (dx > dy ? dx : -dy) / 2;
            for (; ; ) {
                if (start.X == end.X && start.Y == end.Y)
                    yield break;
                yield return start;
                var e2 = er;
                if (e2 > -dx) { er -= dy; start = new(start.X + sx, start.Y); }
                if (e2 < dy) { er += dx; start = new(start.X, start.Y + sy); }
            }
        }
    }
}
