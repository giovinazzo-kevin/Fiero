using Ergo.Lang;
using Fiero.Core;

namespace Fiero.Business
{
    public abstract class DrawableEntity : Entity
    {
        [NonTerm]
        [RequiredComponent]
        public RenderComponent Render { get; private set; }
    }
}
