using Fiero.Core;
using SFML.Window;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class PlayerActionProvider : ActionProvider
    {
        protected readonly GameUI UI;
        protected readonly Queue<IAction> QueuedActions;
        protected Modal CurrentModal { get; private set; }

        public PlayerActionProvider(GameUI ui)
        {
            UI = ui;
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
            if (IsKeyPressed(Data.Hotkeys.Interact)) {
                return new InteractRelativeAction();
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
                return new NoAction();
            }
            return new NoAction();

            IAction MoveOrAttack(Coord c) => wantToAttack ? new AttackDirectionAction(c) : new MoveRelativeAction(c);
            bool IsKeyPressed(GameDatum<Keyboard.Key> datum) => UI.Input.IsKeyPressed(UI.Store.Get(datum));
            bool IsKeyDown(GameDatum<Keyboard.Key> datum) => UI.Input.IsKeyDown(UI.Store.Get(datum));
        }
    }
}
