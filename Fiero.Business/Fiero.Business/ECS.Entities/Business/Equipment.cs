using Fiero.Core;

namespace Fiero.Business
{
    public class Equipment : Item
    {
        [RequiredComponent]
        public EquipmentComponent EquipmentProperties { get; private set; }
    }
}
