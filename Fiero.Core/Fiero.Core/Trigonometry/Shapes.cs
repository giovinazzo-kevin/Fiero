using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Core
{
    public static class Shapes
    {
        public static IEnumerable<Coord> Box(Coord center, int side)
        {
            side /= 2;
            for (int x = -side; x <= side; x++) {
                for (int y = -side; y <= side; y++) {
                    yield return center + new Coord(x, y);
                }
            }
        }

        public static IEnumerable<Coord> Neighborhood(Coord center, int side)
            => Box(center, side).Except(new[] { center });

        public static IEnumerable<Coord> Disc(Coord center, int diameter)
        {
            var radius = diameter / 2;
            var rr = radius * radius;
            for (int x = -radius; x <= radius; x++) {
                for (int y = -radius; y <= radius; y++) {
                    var p = center + new Coord(x, y);
                    if(p.DistSq(center) <= rr) {
                        yield return p;
                    }
                }
            }
        }

        public static IEnumerable<Coord> Circle(Coord center, int radius)
        {
            int d = (5 - radius * 4) / 4;
            int x = 0;
            int y = radius;
            do {
                yield return center + new Coord(x,  y);
                yield return center + new Coord(x,  -y);
                yield return center + new Coord(-x,  y);
                yield return center + new Coord(-x,  -y);
                yield return center + new Coord(y,  x);
                yield return center + new Coord(y,  -x);
                yield return center + new Coord(-y,  x);
                yield return center + new Coord(-y,  -x);
                if (d < 0) {
                    d += 2 * x + 1;
                }
                else {
                    d += 2 * (x - y) + 1;
                    y--;
                }
                x++;
            } while (x <= y);
        }

        public static IEnumerable<Coord> Line(Coord start, Coord end)
        {
            var dx = Math.Abs(end.X - start.X); var sx = start.X < end.X ? 1 : -1;
            var dy = Math.Abs(end.Y - start.Y); var sy = start.Y < end.Y ? 1 : -1;
            var er = (dx > dy ? dx : -dy) / 2;
            for (; ; ) {
                yield return start;
                if (start.X == end.X && start.Y == end.Y)
                    yield break;
                var e2 = er;
                if (e2 > -dx) { er -= dy; start = new(start.X + sx, start.Y); }
                if (e2 < dy) { er += dx; start = new(start.X, start.Y + sy); }
            }
        }
    }
}
