using Fiero.Core;

namespace Fiero.Business
{
    public class PhysicalEntity : DrawableEntity
    {
        [RequiredComponent]
        public PhysicsComponent Physics { get; private set; }
    }
}
