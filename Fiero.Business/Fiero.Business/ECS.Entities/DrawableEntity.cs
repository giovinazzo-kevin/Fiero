using Fiero.Core;

namespace Fiero.Business
{
    public abstract class DrawableEntity : Entity
    {
        [RequiredComponent]
        public RenderComponent Render { get; private set; }
    }
}
