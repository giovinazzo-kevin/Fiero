using Fiero.Core;
using System;
using System.Linq;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleOrganize(ActorTime t, ref IAction action, ref int? cost)
        {
            if (action is DropItemAction drop) {
                return ItemDropped.Handle(new(t.Actor, drop.Item));
            }
            else if (action is EquipItemAction equip) {
                return ItemEquipped.Handle(new(t.Actor, equip.Item));
            }
            else if (action is UnequipItemAction unequip) {
                return ItemUnequipped.Handle(new(t.Actor, unequip.Item));
            }
            else if (action is UseConsumableAction use) {
                return ItemConsumed.Handle(new(t.Actor, use.Item));
            }
            else throw new NotSupportedException();
        }
    }
}
