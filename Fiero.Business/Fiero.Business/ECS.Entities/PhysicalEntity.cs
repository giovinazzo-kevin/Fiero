using Fiero.Core;

namespace Fiero.Business
{
    public abstract class PhysicalEntity : DrawableEntity
    {
        [RequiredComponent]
        public PhysicsComponent Physics { get; private set; }
    }
}
