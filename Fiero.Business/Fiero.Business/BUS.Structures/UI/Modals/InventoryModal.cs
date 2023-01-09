using Fiero.Core;
using SFML.Window;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class InventoryModal : ContainerModal<Actor, InventoryActionName>
    {
        public InventoryModal(GameUI ui, GameResources resources, Actor act)
            : base(ui, resources, act, new[] { ModalWindowButton.Close }, ModalWindowStyles.Default) { }

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
            if (Container.Equipment.IsEquipped(i)) {
                yield return InventoryActionName.Unequip;
            }
            else {
                yield return InventoryActionName.Drop;
                if (i.TryCast<Weapon>(out _) || i.TryCast<Armor>(out _)) {
                    yield return InventoryActionName.Equip;
                }
            }
            if (i.TryCast<Throwable>(out _)) {
                yield return InventoryActionName.Set;
                yield return InventoryActionName.Throw;
            }
            if (i.TryCast<Potion>(out _)) {
                yield return InventoryActionName.Quaff;
                yield return InventoryActionName.Set;
            }
            if (i.TryCast<Scroll>(out _)) {
                yield return InventoryActionName.Read;
                yield return InventoryActionName.Set;
            }
            if (i.TryCast<Wand>(out _)) {
                yield return InventoryActionName.Set;
                yield return InventoryActionName.Zap;
            }
        }
    }
}
