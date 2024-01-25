namespace Fiero.Business
{

    public class Item : PhysicalEntity
    {
        [RequiredComponent]
        public ItemComponent ItemProperties { get; private set; }

        public string DisplayName
        {
            get
            {
                if (this.IsInvalid())
                    return null;
                var name = ItemProperties.Identified
                    ? Info.Name
                    : ItemProperties.UnidentifiedName;
                if (TryCast<Consumable>(out var consumable) && (consumable.ConsumableProperties.MaximumUses != 1 || consumable.ConsumableProperties.RemainingUses == 0))
                {
                    return $"{name} ({consumable.ConsumableProperties.RemainingUses})";
                }
                else if (TryCast<Resource>(out var resource) && resource.ResourceProperties.Amount > 1)
                {
                    return $"{name} ({resource.ResourceProperties.Amount})";
                }
                return name;
            }
        }

        public override string ToString() => DisplayName;
    }
}
