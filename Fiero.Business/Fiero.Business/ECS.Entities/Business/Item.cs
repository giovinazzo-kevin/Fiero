using Fiero.Core;
using System.ComponentModel.DataAnnotations;

namespace Fiero.Business
{
    public class Item : PhysicalEntity
    {
        [RequiredComponent]
        public ItemComponent ItemProperties { get; private set; }

        public string DisplayName {
            get {
                var name = ItemProperties.Identified
                    ? Info.Name
                    : ItemProperties.UnidentifiedName;
                if (TryCast<Consumable>(out var consumable) && consumable.ConsumableProperties.RemainingUses > 1) {
                    return $"{name} ({consumable.ConsumableProperties.RemainingUses})";
                }
                else if (TryCast<Resource>(out var resource) && resource.ResourceProperties.Amount > 1) {
                    return $"{name} ({resource.ResourceProperties.Amount})";
                }
                return name;
            }
        }

        public override string ToString() => DisplayName;
    }
}
