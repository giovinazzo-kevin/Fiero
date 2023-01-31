using Fiero.Core;
using Fiero.Core.Structures;

namespace Fiero.Business
{
    public readonly struct RoomConnector
    {
        public readonly Room Owner;
        public readonly UnorderedPair<Coord> Edge;
        public readonly Coord Center;

        public RoomConnector(Room owner, UnorderedPair<Coord> edge)
        {
            Owner = owner;
            Edge = edge;
            Center = (edge.Left + edge.Right) / 2;
        }
    }
}
