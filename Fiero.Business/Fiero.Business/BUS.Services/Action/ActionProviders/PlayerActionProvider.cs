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
        protected readonly QuickSlotHelper QuickSlots;

        protected Modal CurrentModal { get; private set; }

        public PlayerActionProvider(GameUI ui, GameSystems systems, QuickSlotHelper slots)
        {
            UI = ui;
            Systems = systems;
            QueuedActions = new();
            QuickSlots = slots;
        }

        public override IAction GetIntent(Actor a)
        {
            if (CurrentModal != null)
                return new NoAction();
            if (QueuedActions.TryDequeue(out var backedUp)) {
                return backedUp;
            }
            var floorId = a.FloorId();
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
            if (IsKeyPressed(Data.Hotkeys.Look)) {
                UI.Look();
            }
            if (QuickSlots.TryGetAction(out var action)) {
                return action;
            }
            if (IsKeyPressed(Data.Hotkeys.Inventory)) {
                var inventoryModal = UI.Inventory(a, "Bag");
                CurrentModal = inventoryModal;
                inventoryModal.Closed += (_, __) => CurrentModal = null;
                inventoryModal.ActionPerformed += (item, action) => {
                    inventoryModal.Close(ModalWindowButton.None);
                    switch (action) {
                        case InventoryActionName.Set:
                            Set(item);
                            break;
                        case InventoryActionName.Quaff when item.TryCast<Potion>(out var potion):
                            QueuedActions.Enqueue(new QuaffPotionAction(potion));
                            break;
                        case InventoryActionName.Read when item.TryCast<Scroll>(out var scroll):
                            QueuedActions.Enqueue(new ReadScrollAction(scroll));
                            break;
                        case InventoryActionName.Zap when item.TryCast<Wand>(out var wand) && TryZap(wand, out var zap):
                            QueuedActions.Enqueue(zap);
                            break;
                        case InventoryActionName.Throw when item.TryCast<Throwable>(out var throwable) && TryThrow(throwable, out var @throw):
                            QueuedActions.Enqueue(@throw);
                            break;
                        case InventoryActionName.Equip:
                            QueuedActions.Enqueue(new EquipItemAction(item));
                            break;
                        case InventoryActionName.Unequip:
                            QueuedActions.Enqueue(new UnequipItemAction(item));
                            break;
                        case InventoryActionName.Drop:
                            QueuedActions.Enqueue(new DropItemAction(item));
                            break;
                    }
                };
            }
            return new NoAction();

            void Set(Item item)
            {
                UI.NecessaryChoice(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, "Which quick slot?").OptionChosen += (popup, slot) => {
                    if (item.TryCast<Wand>(out var wand)) {
                        UI.NecessaryChoice(new[] { InventoryActionName.Throw, InventoryActionName.Zap }, "Which action?").OptionChosen += (popup, choice) => {
                            if (choice == InventoryActionName.Throw) {
                                QuickSlots.Set(slot, item, () => {
                                    if(TryThrow(wand, out action)) {
                                        QuickSlots.Unset(slot);
                                        return action;
                                    }
                                    return new FailAction();
                                });
                            }
                            else {
                                QuickSlots.Set(slot, item, () => TryZap(wand, out var action) ? action : new FailAction());
                            }
                        };
                    }
                    else if (item.TryCast<Potion>(out var potion)) {
                        UI.NecessaryChoice(new[] { InventoryActionName.Throw, InventoryActionName.Quaff }, "Which action?").OptionChosen += (popup, choice) => {
                            if (choice == InventoryActionName.Throw) {
                                QuickSlots.Set(slot, item, () => {
                                    if (TryThrow(potion, out action)) {
                                        QuickSlots.Unset(slot);
                                        return action;
                                    }
                                    return new FailAction();
                                });
                            }
                            else {
                                QuickSlots.Set(slot, item, () => {
                                    QuickSlots.Unset(slot);
                                    return new QuaffPotionAction(potion);
                                });
                            }
                        };
                    }
                    else if (item.TryCast<Scroll>(out var scroll)) {
                        UI.NecessaryChoice(new[] { InventoryActionName.Throw, InventoryActionName.Read }, "Which action?").OptionChosen += (popup, choice) => {
                            if (choice == InventoryActionName.Throw) {
                                QuickSlots.Set(slot, item, () => {
                                    if (TryThrow(scroll, out action)) {
                                        QuickSlots.Unset(slot);
                                        return action;
                                    }
                                    return new FailAction();
                                });
                            }
                            else {
                                QuickSlots.Set(slot, item, () => {
                                    QuickSlots.Unset(slot);
                                    return new ReadScrollAction(scroll);
                                });
                            }
                        };
                    }
                    else if (item.TryCast<Throwable>(out var throwable)) {
                        QuickSlots.Set(slot, item, () => {
                            if(TryThrow(throwable, out action)) {
                                if(!throwable.ThrowableProperties.ThrowsUseCharges || throwable.ConsumableProperties.RemainingUses == 1) {
                                    QuickSlots.Unset(slot);
                                }
                                return action;
                            }
                            return new FailAction();
                        });
                    }
                };
            }

            bool TryZap(Wand wand, out IAction action)
            {
                floorId = a.FloorId();
                // All wands use the same targeting shape and have "infinite" range
                var line = Shapes.Line(new(0, 0), new(0, 100)).Skip(1).ToArray();
                var zapShape = new RayTargetingShape(a.Position(), 100);
                zapShape.TryAutoTarget(
                    p => Systems.Floor.GetActorsAt(floorId, p).Any(),
                    p => !Systems.Floor.GetCellAt(floorId, p)?.IsWalkable(null) ?? true
                );
                if (UI.Target(zapShape)) {
                    var points = zapShape.GetPoints().ToArray();
                    foreach (var p in points) {
                        var target = Systems.Floor.GetActorsAt(floorId, p)
                            .FirstOrDefault();
                        if (target != null) {
                            action = new ZapWandAtOtherAction(wand, target);
                            return true;
                        }
                    }
                    // Okay, then
                    action = new ZapWandAtPointAction(wand, points.Last() - a.Position());
                    return true;
                }
                action = default;
                return false;
            }

            bool TryThrow(Throwable throwable, out IAction action)
            {
                floorId = a.FloorId();
                var len = throwable.ThrowableProperties.MaximumRange + 1;
                var line = Shapes.Line(new(0, 0), new(0, len))
                    .Skip(1)
                    .ToArray();
                var throwShape = new RayTargetingShape(a.Position(), len);
                throwShape.TryAutoTarget(
                    p => Systems.Floor.GetActorsAt(floorId, p).Any(b => Systems.Faction.GetRelationships(a, b).Left.IsHostile()),
                    p => !Systems.Floor.GetCellAt(floorId, p)?.IsWalkable(null) ?? true
                );
                if (UI.Target(throwShape)) {
                    var points = throwShape.GetPoints().ToArray();
                    foreach (var p in points) {
                        var target = Systems.Floor.GetActorsAt(floorId, p)
                            .FirstOrDefault(b => Systems.Faction.GetRelationships(a, b).Left.IsHostile());
                        if (target != null) {
                            action = new ThrowItemAtOtherAction(target, throwable);
                            return true;
                        }
                    }
                    // Okay, then
                    action = new ThrowItemAtPointAction(points.Last() - a.Position(), throwable);
                    return true;
                }
                action = default;
                return false;
            }

            IAction MoveOrAttack(Coord c)
            {
                if (wantToAttack) {
                    return new MeleeAttackPointAction(c, a.Equipment.Weapon);
                }
                return new MoveRelativeAction(c);
            }
            bool IsKeyPressed(GameDatum<Keyboard.Key> datum) => UI.Input.IsKeyPressed(UI.Store.Get(datum));
            bool IsKeyDown(GameDatum<Keyboard.Key> datum) => UI.Input.IsKeyDown(UI.Store.Get(datum));
        }
    }
}
