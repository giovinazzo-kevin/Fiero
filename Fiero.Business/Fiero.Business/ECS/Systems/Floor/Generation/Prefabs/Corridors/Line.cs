using Fiero.Core;

namespace Fiero.Business
{
    public class Line
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
}
