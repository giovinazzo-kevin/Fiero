using Ergo.Lang;

namespace Fiero.Business
{
    [TransientDependency]
    public class PlayerActionProvider : ActionProvider
    {
        protected readonly GameUI UI;
        protected readonly Queue<IAction> QueuedActions;
        protected readonly QuickSlotHelper QuickSlots;

        protected Modal CurrentModal { get; private set; }

        public PlayerActionProvider(GameUI ui, MetaSystem systems, QuickSlotHelper slots)
            : base(systems)
        {
            UI = ui;
            QueuedActions = new();
            QuickSlots = slots;
        }

        private volatile bool _requestDelay;
        public override bool RequestDelay => _requestDelay;


        public override bool TryTarget(Actor a, TargetingShape shape, bool _) => UI.Target(shape);
        protected void UnsetSlotIfConsumed(DrawableEntity item, int slot)
        {
            if (item is Consumable c
                && c.ConsumableProperties.ConsumedWhenEmpty
                && c.ConsumableProperties.RemainingUses == 1)
                QuickSlots.Unset(slot);
        }

        public override IAction GetIntent(Actor a)
        {
            base.GetIntent(a);
            if (CurrentModal != null || !UI.Input.IsKeyboardFocusAvailable)
                return new NoAction();
            _requestDelay = true;
            QuickSlots.Refresh();
            if (QueuedActions.TryDequeue(out var backedUp))
            {
                return backedUp;
            }
            if (TryFollowPath(a, out var followPath))
            {
                if (GetClosestHostile(a) != null)
                {
                    a.Ai.Objectives.Clear();
                    a.Ai.Path = null;
                    return new NoAction();
                }
                return followPath;
            }
            _requestDelay = false;
            var floorId = a.FloorId();
            var wantToAttack = IsKeyDown(Data.Hotkeys.Modifier)
                || a.Effects.Active.Any(e => e.Name == EffectName.Entrapment);
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
                    return new NoAction();
                }
                return new FailAction();
            }
            if (IsKeyPressed(Data.Hotkeys.Talk))
            {
                if (UI.Look(a, out var cell))
                {
                    var talkTo = cell.Actors
                        .Where(x => !x.IsPlayer())
                        .FirstOrDefault();
                    if (talkTo != null)
                        return new InitiateConversationAction(talkTo);
                }
                return new FailAction();
            }
            if (IsKeyPressed(Data.Hotkeys.AutoExplore))
            {
                var floor = Systems.Get<DungeonSystem>();
                var visibleItems = a.Fov.VisibleTiles[floorId]
                    .TrySelect(t => (floor.TryGetCellAt(floorId, t, out var c), c))
                    .SelectMany(c => c.Items.Where(i => a.Ai.LikedItems.Any(f => f(i)) && !a.Ai.DislikedItems.Any(f => f(i))))
                    .ToList();
                if (visibleItems.Count > 0)
                {
                    TryPushObjective(a, visibleItems.First());
                }
                // Go to the closest tile with the highest number of unexplored neighbors
                // Stop if you detect any danger at all
                else if (!TryGetUnexploredCandidate(a, out var tile) || !TryPushObjective(a, tile))
                {
                    // We've explored everything we can see without opening doors
                    // so autoexplore will now find the closest closed door and open it
                    var closestClosedDoor = floor.GetAllFeatures(floorId)
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
                        else if (Systems.Get<DungeonSystem>().TryGetClosestFreeTile(floorId, door.Position(), out var doorStep,
                            pred: cell => a.Fov.KnownTiles[floorId].Contains(cell.Tile.Position()) && cell.IsWalkable(a)))
                        {
                            TryPushObjective(a, doorStep.Tile, () => new InteractWithFeatureAction(door));
                        }
                    }
                    else
                    {
                        a.Log.Write($"$DoneExploring$");
                        Systems.Get<RenderSystem>().CenterOn(a);
                    }
                }
            }
            if (IsKeyPressed(Data.Hotkeys.AutoFight))
            {
                if (GetClosestHostile(a) is { } hostile)
                {
                    if (a.IsInMeleeRange(hostile))
                        return new MeleeAttackOtherAction(hostile, a.ActorEquipment.Weapons.ToArray());
                    TryPushObjective(a, hostile, () => new MeleeAttackOtherAction(hostile, a.ActorEquipment.Weapons.ToArray()));
                }
            }
            if (QuickSlots.TryGetAction(out var action))
            {
                return action;
            }
            if (IsKeyPressed(Data.Hotkeys.Inventory))
            {
                var inventoryModal = UI.Inventory(a, $"{a.Info.Name} > Inventory");
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
                        case InventoryActionName.Throw when item.TryCast<Projectile>(out var Projectile) && TryThrow(a, Projectile, out var @throw):
                            QueuedActions.Enqueue(@throw);
                            break;
                        case InventoryActionName.Shoot when item.TryCast<Launcher>(out var Launcher) && TryShoot(a, Launcher, out var shoot):
                            QueuedActions.Enqueue(shoot);
                            break;
                        case InventoryActionName.Equip when item.TryCast<Equipment>(out var equip):
                            QueuedActions.Enqueue(new EquipItemAction(equip));
                            break;
                        case InventoryActionName.Unequip when item.TryCast<Equipment>(out var equip):
                            QueuedActions.Enqueue(new UnequipItemAction(equip));
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
                UI.NecessaryChoice(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, title: "Which quick slot?").OptionChosen += (popup, slot) =>
                {
                    if (item.TryCast<Wand>(out var wand))
                    {
                        UI.NecessaryChoice(new[] { InventoryActionName.Throw, InventoryActionName.Zap }, title: "Which action?").OptionChosen += (popup, choice) =>
                        {
                            if (choice == InventoryActionName.Throw)
                            {
                                QuickSlots.Set(slot, item, nameof(InventoryActionName.Throw), () =>
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
                                QuickSlots.Set(slot, item, nameof(InventoryActionName.Zap), () => TryZap(a, wand, out var action) ? action : new FailAction());
                            }
                        };
                    }
                    else if (item.TryCast<Potion>(out var potion))
                    {
                        UI.NecessaryChoice(new[] { InventoryActionName.Throw, InventoryActionName.Quaff }, title: "Which action?").OptionChosen += (popup, choice) =>
                        {
                            if (choice == InventoryActionName.Throw)
                            {
                                QuickSlots.Set(slot, item, nameof(InventoryActionName.Throw), () =>
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
                                QuickSlots.Set(slot, item, nameof(InventoryActionName.Quaff), () =>
                                {
                                    UnsetSlotIfConsumed(item, slot);
                                    return new QuaffPotionAction(potion);
                                });
                            }
                        };
                    }
                    else if (item.TryCast<Scroll>(out var scroll))
                    {
                        UI.NecessaryChoice(new[] { InventoryActionName.Throw, InventoryActionName.Read }, title: "Which action?").OptionChosen += (popup, choice) =>
                        {
                            if (choice == InventoryActionName.Throw)
                            {
                                QuickSlots.Set(slot, item, nameof(InventoryActionName.Throw), () =>
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
                                QuickSlots.Set(slot, item, nameof(InventoryActionName.Read), () =>
                                {
                                    UnsetSlotIfConsumed(item, slot);
                                    return new ReadScrollAction(scroll);
                                });
                            }
                        };
                    }
                    else if (item.TryCast<Launcher>(out var launcher))
                    {
                        UI.NecessaryChoice(new[] { InventoryActionName.Shoot, InventoryActionName.Equip }, title: "Which action?").OptionChosen += (popup, choice) =>
                        {
                            if (choice == InventoryActionName.Shoot)
                            {
                                QuickSlots.Set(slot, item, nameof(InventoryActionName.Shoot), () => TryShoot(a, launcher, out var action) ? action : new FailAction());
                            }
                            else
                            {
                                QuickSlots.Set(slot, item, nameof(InventoryActionName.Equip), () =>
                                {
                                    return new EquipOrUnequipItemAction(launcher);
                                });
                            }
                        };
                    }
                    else if (item.TryCast<Weapon>(out var weapon))
                    {
                        QuickSlots.Set(slot, weapon, nameof(InventoryActionName.Equip), () =>
                        {
                            return new EquipOrUnequipItemAction(weapon);
                        });
                    }
                    else if (item.TryCast<Armor>(out var armor))
                    {
                        QuickSlots.Set(slot, armor, nameof(InventoryActionName.Equip), () =>
                        {
                            return new EquipOrUnequipItemAction(armor);
                        });
                    }
                    else if (item.TryCast<Projectile>(out var proj))
                    {
                        QuickSlots.Set(slot, item, nameof(InventoryActionName.Throw), () =>
                        {
                            if (TryThrow(a, proj, out action))
                            {
                                if (proj.ProjectileProperties.ThrowsUseCharges)
                                    UnsetSlotIfConsumed(item, slot);
                                else
                                    QuickSlots.Unset(slot);
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
                    return new MeleeAttackPointAction(c, a.ActorEquipment.Weapons.ToArray());
                }
                if (!Systems.Get<DungeonSystem>().TryGetCellAt(a.FloorId(), a.Position() + c, out var cell))
                    return new FailAction();
                // don't check features, we want to bump those
                // don't check pathing, that's for autoexplore
                if (!cell.Tile.IsWalkable(a))
                    return new FailAction();
                return new MoveRelativeAction(c);
            }
            bool IsKeyPressed(GameDatum<VirtualKeys> datum) => UI.Input.IsKeyPressed(UI.Store.Get(datum));
            bool IsKeyDown(GameDatum<VirtualKeys> datum) => UI.Input.IsKeyDown(UI.Store.Get(datum));
        }
    }
}
