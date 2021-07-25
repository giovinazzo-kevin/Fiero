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
                return ItemDropped.Request(new(t.Actor, drop.Item)).All(x => x);
            }
            else if (action is EquipItemAction equip) {
                return ItemEquipped.Request(new(t.Actor, equip.Item)).All(x => x);
            }
            else if (action is UnequipItemAction unequip) {
                return ItemUnequipped.Request(new(t.Actor, unequip.Item)).All(x => x);
            }
            else if (action is UseConsumableAction use) {
                return ItemConsumed.Request(new(t.Actor, use.Item)).All(x => x);
            }
            else throw new NotSupportedException();
        }
    }
}
