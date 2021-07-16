using Fiero.Core;
using System.Collections.Generic;

namespace Fiero.Business
{
    public static class CoordEnumerable
    {
        public static IEnumerable<Coord> Circle(int radius)
        {
            int d = (5 - radius * 4) / 4;
            int x = 0;
            int y = radius;
            do {
                yield return new(x,  y);
                yield return new(x,  -y);
                yield return new(-x,  y);
                yield return new(-x,  -y);
                yield return new(y,  x);
                yield return new(y,  -x);
                yield return new(-y,  x);
                yield return new(-y,  -x);
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
    }
}
