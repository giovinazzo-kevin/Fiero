using Fiero.Core;

namespace Fiero.Business
{
    public class Throwable : Consumable
    {
        [RequiredComponent]
        public ThrowableComponent ThrowableProperties { get; private set; }
    }
}
