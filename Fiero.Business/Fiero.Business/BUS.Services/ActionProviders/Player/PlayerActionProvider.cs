using Ergo.Lang;
using Fiero.Core;
using SFML.Window;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class PlayerActionProvider : ActionProvider
    {
        protected readonly GameUI UI;
        protected readonly Queue<IAction> QueuedActions;
        protected readonly QuickSlotHelper QuickSlots;

        protected Modal CurrentModal { get; private set; }

        public PlayerActionProvider(GameUI ui, GameSystems systems, QuickSlotHelper slots)
            : base(systems)
        {
            UI = ui;
            QueuedActions = new();
            QuickSlots = slots;
        }

        private volatile bool _requestDelay;
        public override bool RequestDelay => _requestDelay;


        public override bool TryTarget(Actor a, TargetingShape shape, bool _) => UI.Target(shape);

        public override IAction GetIntent(Actor a)
        {
            base.GetIntent(a);
            if (CurrentModal != null || !UI.Input.IsKeyboardFocusAvailable)
                return new NoAction();
            _requestDelay = true;
            if (QueuedActions.TryDequeue(out var backedUp))
            {
                return backedUp;
            }
            if (TryFollowPath(a, out var followPath))
            {
                return followPath;
            }
            _requestDelay = false;
            var floorId = a.FloorId();
            var wantToAttack = IsKeyDown(Data.Hotkeys.Modifier);
            if (IsKeyPressed(Data.Hotkeys.MoveNW))
            {
                return MoveOrAttack(new(-1, -1));
            }
            if (IsKeyPressed(Data.Hotkeys.MoveN))
            {
                return MoveOrAttack(new(0, -1));
            }
            if (IsKeyPressed(Data.Hotkeys.MoveNE))
            {
                return MoveOrAttack(new(1, -1));
            }
            if (IsKeyPressed(Data.Hotkeys.MoveW))
            {
                return MoveOrAttack(new(-1, 0));
            }
            if (IsKeyPressed(Data.Hotkeys.Wait))
            {
                return new WaitAction();
            }
            if (IsKeyPressed(Data.Hotkeys.MoveE))
            {
                return MoveOrAttack(new(1, 0));
            }
            if (IsKeyPressed(Data.Hotkeys.MoveSW))
            {
                return MoveOrAttack(new(-1, 1));
            }
            if (IsKeyPressed(Data.Hotkeys.MoveS))
            {
                return MoveOrAttack(new(0, 1));
            }
            if (IsKeyPressed(Data.Hotkeys.MoveSE))
            {
                return MoveOrAttack(new(1, 1));
            }
            if (IsKeyPressed(Data.Hotkeys.Interact))
            {
                return new InteractRelativeAction();
            }
            if (IsKeyPressed(Data.Hotkeys.Look))
            {
                if (UI.Look(a, out var cell))
                {
                    TryPushObjective(a, cell.Tile);
                }
            }
            if (IsKeyPressed(Data.Hotkeys.AutoExplore))
            {
                // Go to the closest tile with the highest number of unexplored neighbors
                // Stop if you detect any danger at all
                if (!TryGetUnexploredCandidate(a, out var tile) || !TryPushObjective(a, tile))
                {
                    // We've explored everything we can see without opening doors
                    // so autoexplore will now find the closest closed door and open it
                    var closestClosedDoor = Systems.Dungeon.GetAllFeatures(floorId)
                        .Where(x => x.IsDoorClosed())
                        .Where(x => a.Fov.KnownTiles[floorId].Contains(x.Physics.Position))
                        .OrderBy(x => x.DistanceFrom(a))
                        .Select(x => Maybe.Some(x))
                        .FirstOrDefault();
                    if (closestClosedDoor.TryGetValue(out var door))
                    {
                        // Just open the door
                        if (door.Position().CardinallyAdjacent(a.Position()))
                        {
                            return new InteractWithFeatureAction(door);
                        }
                        // Look for the doorstep, then open the door
                        else if (Systems.Dungeon.TryGetClosestFreeTile(floorId, door.Position(), out var doorStep,
                            pred: cell => a.Fov.KnownTiles[floorId].Contains(cell.Tile.Position()) && cell.IsWalkable(a)))
                        {
                            TryPushObjective(a, doorStep.Tile, () => new InteractWithFeatureAction(door));
                        }
                    }
                    else
                    {
                        a.Log.Write($"$DoneExploring$");
                        Systems.Render.CenterOn(a);
                    }
                }
            }
            if (IsKeyPressed(Data.Hotkeys.AutoFight))
            {
                if (GetClosestHostile(a) is { } hostile)
                {
                    if (a.IsInMeleeRange(hostile))
                        return new MeleeAttackOtherAction(hostile, a.Equipment.Weapon);
                    TryPushObjective(a, hostile, () => new MeleeAttackOtherAction(hostile, a.Equipment.Weapon));
                }
            }
            if (QuickSlots.TryGetAction(out var action))
            {
                return action;
            }
            if (IsKeyPressed(Data.Hotkeys.Inventory))
            {
                var inventoryModal = UI.Inventory(a, "Bag");
                CurrentModal = inventoryModal;
                inventoryModal.Closed += (_, __) => CurrentModal = null;
                inventoryModal.ActionPerformed += (item, action) =>
                {
                    inventoryModal.Close(ModalWindowButton.None);
                    switch (action)
                    {
                        case InventoryActionName.Set:
                            Set(item);
                            break;
                        case InventoryActionName.Quaff when item.TryCast<Potion>(out var potion):
                            QueuedActions.Enqueue(new QuaffPotionAction(potion));
                            break;
                        case InventoryActionName.Read when item.TryCast<Scroll>(out var scroll):
                            QueuedActions.Enqueue(new ReadScrollAction(scroll));
                            break;
                        case InventoryActionName.Zap when item.TryCast<Wand>(out var wand) && TryZap(a, wand, out var zap):
                            QueuedActions.Enqueue(zap);
                            break;
                        case InventoryActionName.Throw when item.TryCast<Throwable>(out var throwable) && TryThrow(a, throwable, out var @throw):
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
                UI.NecessaryChoice(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, "Which quick slot?").OptionChosen += (popup, slot) =>
                {
                    if (item.TryCast<Wand>(out var wand))
                    {
                        UI.NecessaryChoice(new[] { InventoryActionName.Throw, InventoryActionName.Zap }, "Which action?").OptionChosen += (popup, choice) =>
                        {
                            if (choice == InventoryActionName.Throw)
                            {
                                QuickSlots.Set(slot, item, () =>
                                {
                                    if (TryThrow(a, wand, out action))
                                    {
                                        QuickSlots.Unset(slot);
                                        return action;
                                    }
                                    return new FailAction();
                                });
                            }
                            else
                            {
                                QuickSlots.Set(slot, item, () => TryZap(a, wand, out var action) ? action : new FailAction());
                            }
                        };
                    }
                    else if (item.TryCast<Potion>(out var potion))
                    {
                        UI.NecessaryChoice(new[] { InventoryActionName.Throw, InventoryActionName.Quaff }, "Which action?").OptionChosen += (popup, choice) =>
                        {
                            if (choice == InventoryActionName.Throw)
                            {
                                QuickSlots.Set(slot, item, () =>
                                {
                                    if (TryThrow(a, potion, out action))
                                    {
                                        QuickSlots.Unset(slot);
                                        return action;
                                    }
                                    return new FailAction();
                                });
                            }
                            else
                            {
                                QuickSlots.Set(slot, item, () =>
                                {
                                    QuickSlots.Unset(slot);
                                    return new QuaffPotionAction(potion);
                                });
                            }
                        };
                    }
                    else if (item.TryCast<Scroll>(out var scroll))
                    {
                        UI.NecessaryChoice(new[] { InventoryActionName.Throw, InventoryActionName.Read }, "Which action?").OptionChosen += (popup, choice) =>
                        {
                            if (choice == InventoryActionName.Throw)
                            {
                                QuickSlots.Set(slot, item, () =>
                                {
                                    if (TryThrow(a, scroll, out action))
                                    {
                                        QuickSlots.Unset(slot);
                                        return action;
                                    }
                                    return new FailAction();
                                });
                            }
                            else
                            {
                                QuickSlots.Set(slot, item, () =>
                                {
                                    QuickSlots.Unset(slot);
                                    return new ReadScrollAction(scroll);
                                });
                            }
                        };
                    }
                    else if (item.TryCast<Throwable>(out var throwable))
                    {
                        QuickSlots.Set(slot, item, () =>
                        {
                            if (TryThrow(a, throwable, out action))
                            {
                                if (!throwable.ThrowableProperties.ThrowsUseCharges || throwable.ConsumableProperties.RemainingUses == 1)
                                {
                                    QuickSlots.Unset(slot);
                                }
                                return action;
                            }
                            return new FailAction();
                        });
                    }
                };
            }
            IAction MoveOrAttack(Coord c)
            {
                if (wantToAttack)
                {
                    return new MeleeAttackPointAction(c, a.Equipment.Weapon);
                }
                if (!Systems.Dungeon.TryGetCellAt(a.FloorId(), a.Position() + c, out var cell))
                    return new FailAction();
                // don't check features, we want to bump those
                // don't check pathing, that's for autoexplore
                if (cell.Tile.Physics.BlocksMovement)
                    return new FailAction();
                return new MoveRelativeAction(c);
            }
            bool IsKeyPressed(GameDatum<Keyboard.Key> datum) => UI.Input.IsKeyPressed(UI.Store.Get(datum));
            bool IsKeyDown(GameDatum<Keyboard.Key> datum) => UI.Input.IsKeyDown(UI.Store.Get(datum));
        }
    }
}
