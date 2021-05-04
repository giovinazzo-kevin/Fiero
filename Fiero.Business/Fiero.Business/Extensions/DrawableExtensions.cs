using Fiero.Core;
using System.Drawing;

namespace Fiero.Business
{
    public static class DrawableExtensions
    {
        public static double DistanceFrom(this Drawable a, Drawable b)
            => a.DistanceFrom(b.Physics.Position);
        public static double DistanceFrom(this Drawable a, Coord pos)
            => a.Physics.Position.Dist(pos);
    }
}
