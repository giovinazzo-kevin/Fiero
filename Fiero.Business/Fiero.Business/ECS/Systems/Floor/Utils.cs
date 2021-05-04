using System;

namespace Fiero.Business
{
    public static class Utils
    {
        public static bool Bresenham((int X, int Y) start, (int X, int Y) end, Func<int, int, bool> callback)
        {
            var dx = Math.Abs(end.X - start.X); var sx = start.X < end.X ? 1 : -1;
            var dy = Math.Abs(end.Y - start.Y); var sy = start.Y < end.Y ? 1 : -1;
            var er = (dx > dy ? dx : -dy) / 2;
            for (; ; ) {
                if (!callback(start.X, start.Y))
                    return false;
                if (start.X == end.X && start.Y == end.Y)
                    break;
                var e2 = er;
                if (e2 > -dx) { er -= dy; start = (start.X + sx, start.Y); }
                if (e2 < dy) { er += dx; start = (start.X, start.Y + sy); }
            }
            return true;
        }
    }
}
