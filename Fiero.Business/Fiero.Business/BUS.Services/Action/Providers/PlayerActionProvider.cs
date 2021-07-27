using Fiero.Core;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class PlayerActionProvider : ActionProvider
    {
        protected readonly GameUI UI;
        protected readonly GameSystems Systems;
        protected readonly Queue<IAction> QueuedActions;
        protected Modal CurrentModal { get; private set; }

        public PlayerActionProvider(GameUI ui, GameSystems systems)
        {
            UI = ui;
            Systems = systems;
            QueuedActions = new();
        }

        public override IAction GetIntent(Actor a)
        {
            if (CurrentModal != null)
                return new NoAction();
            if(QueuedActions.TryDequeue(out var backedUp)) {
                return backedUp;
            }
            var wantToAttack = IsKeyDown(Data.Hotkeys.Modifier);

            if (IsKeyPressed(Data.Hotkeys.MoveNW)) {
                return MoveOrAttack(new(-1, -1));
            }
            if (IsKeyPressed(Data.Hotkeys.MoveN)) {
                return MoveOrAttack(new(0, -1));
            }
            if (IsKeyPressed(Data.Hotkeys.MoveNE)) {
                return MoveOrAttack(new(1, -1));
            }
            if (IsKeyPressed(Data.Hotkeys.MoveW)) {
                return MoveOrAttack(new(-1, 0));
            }
            if (IsKeyPressed(Data.Hotkeys.Wait)) {
                return new WaitAction();
            }
            if (IsKeyPressed(Data.Hotkeys.MoveE)) {
                return MoveOrAttack(new(1, 0));
            }
            if (IsKeyPressed(Data.Hotkeys.MoveSW)) {
                return MoveOrAttack(new(-1, 1));
            }
            if (IsKeyPressed(Data.Hotkeys.MoveS)) {
                return MoveOrAttack(new(0, 1));
            }
            if (IsKeyPressed(Data.Hotkeys.MoveSE)) {
                return MoveOrAttack(new(1, 1));
            }
            if (IsKeyPressed(Data.Hotkeys.QuickCast1)) {
                if (a.Spells.KnownSpells.ElementAtOrDefault(0) is { } spell) {
                    return new CastSpellAction(spell);
                }
            }
            if (IsKeyPressed(Data.Hotkeys.QuickCast2)) {
                if (a.Spells.KnownSpells.ElementAtOrDefault(1) is { } spell) {
                    return new CastSpellAction(spell);
                }
            }
            if (IsKeyPressed(Data.Hotkeys.QuickCast3)) {
                if (a.Spells.KnownSpells.ElementAtOrDefault(2) is { } spell) {
                    return new CastSpellAction(spell);
                }
            }
            if (IsKeyPressed(Data.Hotkeys.QuickCast4)) {
                if (a.Spells.KnownSpells.ElementAtOrDefault(3) is { } spell) {
                    return new CastSpellAction(spell);
                }
            }
            if (IsKeyPressed(Data.Hotkeys.Interact)) {
                return new InteractRelativeAction();
            }
            if (IsKeyPressed(Data.Hotkeys.Look)) {
                UI.Look(out _);
            }
            if (IsKeyPressed(Data.Hotkeys.FireWeapon)) {
                var rangedWeapons = a.Equipment.GetEquipedWeapons(w => w.AttackType == AttackName.Ranged);
                if (rangedWeapons.Any()) {
                    var possibleTargets = Systems.Floor.GetAllActors(a.FloorId())
                        .Except(new[] { a })
                        .Where(b => a.CanSee(b) && a.IsHostileTowards(b))
                        .Select(t => t.Position())
                        .OrderBy(p => p.DistSq(a.Position()));
                    if (possibleTargets.Any() && UI.Target(possibleTargets.ToArray(), out var cursor)) {
                        return new RangedAttackPointAction(cursor - a.Position(), rangedWeapons.ToArray());
                    }
                }
            }
            if (IsKeyPressed(Data.Hotkeys.Inventory)) {
                var inventoryModal = UI.Inventory(a);
                CurrentModal = inventoryModal;
                inventoryModal.Closed += (_, __) => CurrentModal = null;
                inventoryModal.ActionPerformed += (item, action) => {
                    inventoryModal.Close(ModalWindowButton.None);
                    switch(action) {
                        case InventoryActionName.Drop:
                            QueuedActions.Enqueue(new DropItemAction(item));
                            break;
                        case InventoryActionName.Use when item.TryCast<Consumable>(out var consumable):
                            QueuedActions.Enqueue(new UseConsumableAction(consumable));
                            break;
                        case InventoryActionName.Equip:
                            QueuedActions.Enqueue(new EquipItemAction(item));
                            break;
                        case InventoryActionName.Unequip:
                            QueuedActions.Enqueue(new UnequipItemAction(item));
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                };
            }
            return new NoAction();

            IAction MoveOrAttack(Coord c)
            {
                var meleeWeapons = a.Equipment.GetEquipedWeapons(w => w.AttackType == AttackName.Melee);
                if(wantToAttack) {
                    return new MeleeAttackPointAction(c, meleeWeapons.ToArray());
                }
                return new MoveRelativeAction(c);
            }
            bool IsKeyPressed(GameDatum<Keyboard.Key> datum) => UI.Input.IsKeyPressed(UI.Store.Get(datum));
            bool IsKeyDown(GameDatum<Keyboard.Key> datum) => UI.Input.IsKeyDown(UI.Store.Get(datum));
        }
    }
}
