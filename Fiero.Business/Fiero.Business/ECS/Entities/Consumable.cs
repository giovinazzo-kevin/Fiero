using Fiero.Core;

namespace Fiero.Business
{
    public class Consumable : Item
    {
        [RequiredComponent]
        public ConsumableComponent ConsumableProperties { get; private set; }
    }
}
