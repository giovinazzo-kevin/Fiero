using Fiero.Core;
using System;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        protected bool TryUseItem(Item item, Actor actor, out bool consumed)
        {
            var used = false;
            consumed = false;
            if (item.TryCast<Consumable>(out var consumable)) {
                if (consumable.TryCast<Potion>(out var potion)
                && TryApply(potion.PotionProperties.Effect)) {
                    used = TryConsume(out consumed);
                }
                if (consumable.TryCast<Scroll>(out var scroll)
                && TryApply(scroll.ScrollProperties.Effect)) {
                    used = TryConsume(out consumed);
                }
            }
            if (consumed) {
                // Assumes item was used from inventory
                _ = actor.Inventory.TryTake(item);
            }
            return used;

            bool TryConsume(out bool consumed)
            {
                consumed = false;
                if (consumable.ConsumableProperties.RemainingUses <= 0) {
                    return false;
                }
                if (--consumable.ConsumableProperties.RemainingUses <= 0
                 && consumable.ConsumableProperties.ConsumedWhenEmpty) {
                    consumed = true;
                }
                return true;
            }

            bool TryApply(EffectName effect)
            {
                switch (effect) {
                    default: return true;
                }
            }
        }


        protected virtual bool HandleOrganize(Actor actor, ref IAction action, ref int? cost)
        {
            if (action is DropItemAction drop)
                return HandleDropItem(drop.Item);
            if (action is EquipItemAction equip)
                return HandleEquipItem(equip.Item);
            if (action is UnequipItemAction unequip)
                return HandleUnequipItem(unequip.Item);
            if (action is UseConsumableAction use)
                return HandleUseConsumable(use.Item);
            throw new NotSupportedException();
            bool HandleDropItem(Item item)
            {
                _ = actor.Inventory.TryTake(item);
                if (_floorSystem.TryGetClosestFreeTile(actor.Physics.Position, out var tile)) {
                    item.Physics.Position = tile.Physics.Position;
                    _floorSystem.CurrentFloor.AddItem(item.Id);
                    actor.Log?.Write($"$Action.YouDrop$ {item.DisplayName}.");
                }
                else {
                    actor.Log?.Write($"$Action.NoSpaceToDrop$ {item.DisplayName}.");
                }
                return true;
            }

            bool HandleEquipItem(Item item)
            {
                if (actor.Equipment.TryEquip(item)) {
                    actor.Log?.Write($"$Action.YouEquip$ {item.DisplayName}.");
                }
                else {
                    actor.Log?.Write($"$Action.YouFailEquipping$ {item.DisplayName}.");
                }
                return true;
            }

            bool HandleUnequipItem(Item item)
            {
                if (actor.Equipment.TryUnequip(item)) {
                    actor.Log?.Write($"$Action.YouUnequip$ {item.DisplayName}.");
                }
                else {
                    actor.Log?.Write($"$Action.YouFailUnequipping$ {item.DisplayName}.");
                }
                return true;
            }

            bool HandleUseConsumable(Item item)
            {
                if (TryUseItem(item, actor, out var consumed)) {
                    actor.Log?.Write($"$Action.YouUse$ {item.DisplayName}.");
                }
                else {
                    actor.Log?.Write($"$Action.YouFailUsing$ {item.DisplayName}.");
                }
                if (consumed) {
                    actor.Log?.Write($"$Action.AnItemIsConsumed$ {item.DisplayName}.");
                }
                return true;
            }
        }
    }
}
