using Fiero.Core;

namespace Fiero.Business
{
    public class Line
    {
        public readonly int A, B, C;

        public readonly Coord Start, End, Middle;

        public Line(Coord a, Coord b)
        {
            Start = a; End = b; Middle = (a + b) / 2;
            A = (a.Y - b.Y);
            B = (b.X - a.X);
            C = (a.X * b.Y - b.X * a.Y);
        }

        public bool IsParallel(Line other) => A * other.B - B * other.A == 0;
        public bool IsPerpendicular(Line other)
        {
            if (this.B != 0 && other.B != 0)
            {
                return this.A * other.A + this.B * other.B == 0;
            }
            else
            {
                // If either B is zero, it means the line is vertical. So, the other line is 
                // perpendicular to it only if it is horizontal (i.e., its A is zero)
                return (this.B == 0 && other.A == 0) || (other.B == 0 && this.A == 0);
            }
        }
        public bool Intersection(Line other, out Coord p)
        {
            var D = A * other.B - B * other.A;
            var Dx = C * other.B - B * other.C;
            var Dy = A * other.C - C * other.A;
            if (D != 0)
            {
                p = new(Dx / D, Dy / D);
                return true;
            }
            p = default;
            return false;
        }
    }
}
