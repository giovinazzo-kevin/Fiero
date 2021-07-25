using Fiero.Core;

namespace Fiero.Business
{
    public abstract class Drawable : Entity
    {
        [RequiredComponent]
        public RenderComponent Render { get; private set; }
        [RequiredComponent]
        public PhysicsComponent Physics { get; private set; }
    }
}
