using Fiero.Core;

namespace Fiero.Business
{
    public class Spell : DrawableEntity
    {
        [RequiredComponent]
        public SpellComponent SpellProperties { get; private set; }
    }
}
