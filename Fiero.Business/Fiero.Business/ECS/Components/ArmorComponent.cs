using Fiero.Core;

namespace Fiero.Business
{
    public class ArmorComponent : Component
    {
        public ArmorName Type { get; set; }
        public ArmorSlotName Slot { get; set; }
    }
}
