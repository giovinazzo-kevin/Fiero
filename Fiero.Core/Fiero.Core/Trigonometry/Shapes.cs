namespace Fiero.Core
{
    public static class Shapes
    {
        public static IEnumerable<Coord> Box(Coord center, int side)
        {
            int halfSide = side / 2;
            int start = -halfSide;
            int end = halfSide;

            if (side % 2 == 0)
                end--;

            for (int x = start; x <= end; x++)
            {
                for (int y = start; y <= end; y++)
                {
                    yield return center + new Coord(x, y);
                }
            }
        }
        public static IEnumerable<Coord> Rect(Coord topLeft, Coord size)
        {
            for (int x = 0; x <= size.X; x++)
            {
                for (int y = 0; y <= size.Y; y++)
                {
                    yield return topLeft + new Coord(x, y);
                }
            }
        }

        public static IEnumerable<Coord> Neighborhood(Coord center, int side)
            => Box(center, side).Except(new[] { center });
        public static IEnumerable<Coord> SquareSpiral(Coord center, int n)
        {
            yield return center;
            var m = n; n = 0;
            while (n++ <= m)
            {
                var r = Math.Floor((Math.Sqrt(n + 1) - 1) / 2) + 1;
                // compute radius : inverse arithmetic sum of 8+16+24+...=
                var p = 8 * r * (r - 1) / 2;
                // compute total point on radius -1 : arithmetic sum of 8+16+24+...
                var en = r * 2;
                // points by face
                var a = (1 + n - p) % (r * 8);
                // compute de position and shift it so the first is (-r,-r) but (-r+1,-r) so square can connect
                var pos = Coord.Zero;
                switch (Math.Floor(a / (r * 2)))
                {
                    // find the face : 0 top, 1 right, 2, bottom, 3 left
                    case 0:
                        {
                            pos = new((int)(a - r), (int)(-r));
                        }
                        break;
                    case 1:
                        {
                            pos = new((int)(r), (int)(a % en - r));
                        }
                        break;
                    case 2:
                        {
                            pos = new((int)(r - a % en), (int)(r));
                        }
                        break;
                    case 3:
                        {
                            pos = new((int)(-r), (int)(r - a % en));
                        }
                        break;
                }
                yield return pos + center;
            }
        }

        public static IEnumerable<Coord> Disc(Coord center, float diameter)
        {
            var radius = diameter / 2;
            var rr = radius * radius;
            for (float x = -radius; x <= radius; x++)
            {
                for (float y = -radius; y <= radius; y++)
                {
                    var p = center + new Coord((int)x, (int)y);
                    if (p.DistSq(center) <= rr)
                    {
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
            do
            {
                yield return center + new Coord(x, y);
                yield return center + new Coord(x, -y);
                yield return center + new Coord(-x, y);
                yield return center + new Coord(-x, -y);
                yield return center + new Coord(y, x);
                yield return center + new Coord(y, -x);
                yield return center + new Coord(-y, x);
                yield return center + new Coord(-y, -x);
                if (d < 0)
                {
                    d += 2 * x + 1;
                }
                else
                {
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
            for (; ; )
            {
                yield return start;
                if (start.X == end.X && start.Y == end.Y)
                    yield break;
                var e2 = er;
                if (e2 > -dx) { er -= dy; start = new(start.X + sx, start.Y); }
                if (e2 < dy) { er += dx; start = new(start.X, start.Y + sy); }
            }
        }

        public static IEnumerable<Coord> ThickLine(Coord start, Coord end, int thickness)
        {
            if (thickness <= 0)
                return Enumerable.Empty<Coord>();
            var len = start.DistChebyshev(end);
            return Line(start, end)
                .SelectMany(p => Box(p, thickness))
                .Where(p => p.DistChebyshev(start) <= len && p.DistChebyshev(end) <= len);
        }
    }
}
