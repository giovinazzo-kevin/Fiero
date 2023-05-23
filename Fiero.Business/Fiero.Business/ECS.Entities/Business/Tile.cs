using Fiero.Core;

namespace Fiero.Business
{

    public class Tile : PhysicalEntity, IPathNode<PhysicalEntity>
    {
        [RequiredComponent]
        public TileComponent TileProperties { get; private set; }

        public double GetCost(PhysicalEntity e) => TileProperties.PathingCost;
        public bool IsWalkable(PhysicalEntity actor) => actor.Physics.Phasing || !Physics.BlocksMovement;
    }
}
