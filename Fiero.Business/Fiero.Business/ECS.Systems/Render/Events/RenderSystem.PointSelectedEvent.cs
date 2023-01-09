using Fiero.Core;

namespace Fiero.Business
{
    public partial class RenderSystem
    {
        public readonly struct PointSelectedEvent
        {
            public readonly Coord Point;
            public PointSelectedEvent(Coord c)
                => (Point) = (c);
        }
    }
}
