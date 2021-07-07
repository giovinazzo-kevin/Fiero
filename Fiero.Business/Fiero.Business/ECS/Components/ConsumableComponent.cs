using Fiero.Core;

namespace Fiero.Business
{
    public class ConsumableComponent : Component
    {
        public int RemainingUses { get; set; }
        public int MaximumUses { get; set; }
        public bool ConsumedWhenEmpty { get; set; }
    }
}
