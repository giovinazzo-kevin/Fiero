using Fiero.Core;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class PlayerActionProvider : ActionProvider
    {
        protected static readonly GameDatum<Keyboard.Key>[] QuickCastSlotKeys = new[] {
            Data.Hotkeys.QuickCast1, Data.Hotkeys.QuickCast2, Data.Hotkeys.QuickCast3, Data.Hotkeys.QuickCast4
        };

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
            for (int i = 0; i < QuickCastSlotKeys.Length; i++) {
                if (IsKeyPressed(QuickCastSlotKeys[i])) {
                    if (a.Spells.KnownSpells.ElementAtOrDefault(i) is { } spell) {
                        foreach (var effect in spell.Effects.Active) {
                            if (effect is CastEffect cast && !cast.ShouldApply(Systems, spell, a)) {
                                return new FailAction();
                            }
                        }
                        if (UI.Target(spell.SpellProperties.TargetingShape, out var target)) {
                            return new CastSpellAction(spell, target);
                        }
                    }
                }
            }
            if (IsKeyPressed(Data.Hotkeys.Interact)) {
                return new InteractRelativeAction();
            }
            if (IsKeyPressed(Data.Hotkeys.Look)) {
                UI.Look(out _);
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
                        case InventoryActionName.Throw when item.TryCast<Throwable>(out var throwable):
                            var len = throwable.ThrowableProperties.MaximumRange + 1;
                            var line = Shapes.Line(new(0, 0), new(0, len)).Skip(1).ToArray();
                            var throwShape = new TargetingShape(0, true, line);
                            if (UI.Target(throwShape, out throwShape)) {
                                foreach (var p in throwShape.Points) {
                                    var target = Systems.Floor.GetActorsAt(floorId, p)
                                    .FirstOrDefault(b => Systems.Faction.GetRelationships(a, b).Left.IsHostile());
                                    if (target != null) {
                                        QueuedActions.Enqueue(new ThrowItemAtOtherAction(target, throwable));
                                        return;
                                    }
                                }
                                // Okay, then
                                QueuedActions.Enqueue(new ThrowItemAtPointAction(throwShape.Points.Last() - a.Position(), throwable));
                            }
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
                if(wantToAttack) {
                    return new MeleeAttackPointAction(c, a.Equipment.Weapon);
                }
                return new MoveRelativeAction(c);
            }
            bool IsKeyPressed(GameDatum<Keyboard.Key> datum) => UI.Input.IsKeyPressed(UI.Store.Get(datum));
            bool IsKeyDown(GameDatum<Keyboard.Key> datum) => UI.Input.IsKeyDown(UI.Store.Get(datum));
        }
    }
}
