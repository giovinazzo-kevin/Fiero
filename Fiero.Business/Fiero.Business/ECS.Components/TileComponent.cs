using Fiero.Core;

namespace Fiero.Business
{
    public class TileComponent : EcsComponent
    {
        /// <summary>
        /// How good this tile looks as a pathfinding candidate
        /// </summary>
        public double PathingCost => MovementCost / 100d; // increases by 1 for every extra turn it takes to wade through this tile
        /// <summary>
        /// The amount of ticks it takes to cross this tile. A turn equals 100 ticks.
        /// </summary>
        public int MovementCost { get; set; }
        public TileName Name { get; set; }
    }
}
