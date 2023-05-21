namespace Fiero.Core
{
    public class Line
    {
        public enum Side
        {
            Left,
            Right,
            OnLine
        }
        public readonly int A, B, C;

        public readonly Coord Start, End, Middle;

        public Line(Coord a, Coord b)
        {
            Start = a; End = b; Middle = (a + b) / 2;
            A = (a.Y - b.Y);
            B = (b.X - a.X);
            C = (a.X * b.Y - b.X * a.Y);
        }
        public Side DeterminePointSide(Coord point)
        {
            int crossProduct = (End.X - Start.X) * (point.Y - Start.Y) - (End.Y - Start.Y) * (point.X - Start.X);
            if (crossProduct > 0)
                return Side.Left;
            else if (crossProduct < 0)
                return Side.Right;
            else
                return Side.OnLine;
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

        public Line Contract(float amount)
        {
            // Calculate the total length of the line
            float totalLength = (float)Start.Dist(End);

            // Determine how much to move towards the middle for start and end points
            float moveAmount = totalLength * amount / 2;

            // Calculate the new start point
            Vec newStart = new(
                Start.X + ((Middle.X - Start.X) * (moveAmount / totalLength)),
                Start.Y + ((Middle.Y - Start.Y) * (moveAmount / totalLength))
            );

            // Calculate the new end point
            Vec newEnd = new(
                End.X - ((End.X - Middle.X) * (moveAmount / totalLength)),
                End.Y - ((End.Y - Middle.Y) * (moveAmount / totalLength))
            );

            // Create and return the new contracted Line
            return new Line(newStart.Round().ToCoord(), newEnd.Round().ToCoord());
        }
    }
}
