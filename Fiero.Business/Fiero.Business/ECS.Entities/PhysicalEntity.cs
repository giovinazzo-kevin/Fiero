using Ergo.Lang;
using Fiero.Core;

namespace Fiero.Business
{
    public class PhysicalEntity : DrawableEntity
    {
        [NonTerm]
        [RequiredComponent]
        public PhysicsComponent Physics { get; private set; }
        [NonTerm]
        public InventoryComponent Inventory { get; private set; }
    }
}
