namespace Fiero.Business
{
    public class RoomConnector
    {
        public readonly Room Owner;
        public readonly UnorderedPair<Coord> Edge;
        public readonly Coord Middle;
        /// <summary>
        /// An external generator will set this to true before drawing if the connector is being actively used.
        /// </summary>
        public bool IsUsed { get; set; }
        /// <summary>
        /// If true, this connector will not be drawn as a wall but as ground
        /// </summary>
        public bool IsShared { get; set; }

        public RoomConnector(Room owner, UnorderedPair<Coord> edge)
        {
            Owner = owner;
            Edge = edge;
            Middle = (edge.Left + edge.Right) / 2;
        }
    }
}
