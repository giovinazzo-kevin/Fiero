using Fiero.Core;

namespace Fiero.Business
{

    public class Tile : PhysicalEntity, IPathNode<object>
    {
        [RequiredComponent]
        public TileComponent TileProperties { get; private set; }

        public bool IsWalkable(object inContext) => !Physics.BlocksMovement;
    }
}
