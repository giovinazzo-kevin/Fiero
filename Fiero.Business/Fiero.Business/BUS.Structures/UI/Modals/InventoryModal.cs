namespace Fiero.Business
{
    public class InventoryModal : ContainerModal<Actor, InventoryActionName>
    {
        public InventoryModal(GameUI ui, GameResources resources, Actor act)
            : base(ui, resources, act, new ModalWindowButton[] { }) { }

        protected override bool ShouldRemoveItem(Item i, InventoryActionName a)
        {
            var shouldRemoveMenuItem = a == InventoryActionName.Drop;
            var consumingAction = new[] { InventoryActionName.Read, InventoryActionName.Throw, InventoryActionName.Zap, InventoryActionName.Quaff }
                .Contains(a);
            shouldRemoveMenuItem |= consumingAction && i.TryCast<Consumable>(out var c)
                && c.ConsumableProperties.ConsumedWhenEmpty && c.ConsumableProperties.RemainingUses == 1;
            return shouldRemoveMenuItem;
        }

        protected override IEnumerable<InventoryActionName> GetAvailableActions(Item i)
        {
            var canDrop = true;
            if (i.TryCast<Equipment>(out var eq))
            {
                if (Container.ActorEquipment.IsEquipped(eq))
                {
                    canDrop = false;
                    yield return InventoryActionName.Unequip;
                }
                else
                {
                    yield return InventoryActionName.Equip;
                }
                yield return InventoryActionName.Set;
            }
            if (i.TryCast<Projectile>(out _))
            {
                yield return InventoryActionName.Throw;
                yield return InventoryActionName.Set;
            }
            if (i.TryCast<Potion>(out _))
            {
                yield return InventoryActionName.Quaff;
                yield return InventoryActionName.Set;
            }
            if (i.TryCast<Scroll>(out _))
            {
                yield return InventoryActionName.Read;
                yield return InventoryActionName.Set;
            }
            if (i.TryCast<Wand>(out _))
            {
                yield return InventoryActionName.Zap;
                yield return InventoryActionName.Set;
            }
            if (i.TryCast<Launcher>(out _))
            {
                yield return InventoryActionName.Shoot;
                yield return InventoryActionName.Set;
            }
            if (canDrop)
            {
                yield return InventoryActionName.Drop;
            }
        }
    }
}
