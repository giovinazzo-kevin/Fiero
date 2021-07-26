using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Transactions;
using Unconcern.Common;
using static SFML.Window.Keyboard;

namespace Fiero.Business.Scenes
{
    public class GameplayScene : GameScene<GameplayScene.SceneState>
    {
        public enum SceneState
        {
            [EntryState]
            Main,
            [ExitState]
            Exit_GameOver,
            [ExitState]
            Exit_SaveAndQuit
        }

        protected readonly GameInput Input; // TODO: Remove
        protected readonly GameSystems Systems;
        protected readonly GameResources Resources;
        protected readonly GameDataStore Store;
        protected readonly GameEntities Entities;
        protected readonly OffButton OffButton;
        protected readonly GameUI UI;

        public Actor Player { get; private set; }

        public GameplayScene(
            GameInput input,
            GameDataStore store,
            GameEntities entities,
            GameSystems systems,
            GameResources resources,
            GameUI ui,
            OffButton off)
        {
            Input = input;
            Store = store;
            Entities = entities;
            Systems = systems;
            Resources = resources;
            UI = ui;
            OffButton = off;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            SubscribeDialogueHandlers();
            Systems.Render.Initialize();
            Systems.Render.Initialize();

            void SubscribeDialogueHandlers()
            {
                Resources.Dialogues.GetDialogue(NpcName.GreatKingRat, GKRDialogueName.JustMet)
                    .Triggered += (t, eh) => {
                        Resources.Sounds.Get(SoundName.BossSpotted).Play();
                    };
                Resources.Dialogues.GetDialogue(NpcName.GreatKingRat, GKRDialogueName.JustMet_Friend)
                    .Triggered += (t, eh) => {
                        foreach (var player in eh.DialogueListeners.Players()) {
                            Systems.Faction.UpdateRelationship(FactionName.Rats, FactionName.Players,
                                x => x.With(StandingName.Loved), out _);
                        }
                    };
                Resources.Dialogues.GetDialogue(NpcName.GreatKingRat, GKRDialogueName.JustMet_Enemy)
                    .Triggered += (t, eh) => {
                        var gkr = (Actor)eh.DialogueStarter;
                        foreach (var player in eh.DialogueListeners.Players()) {
                            Systems.Faction.UpdateRelationship(FactionName.Rats, FactionName.Players,
                                x => x.With(StandingName.Hated), out _);
                            player.ActorProperties.Relationships.Update(gkr,
                                x => x.With(StandingName.Hated), out _);
                            gkr.ActorProperties.Relationships.Update(player,
                                x => x.With(StandingName.Hated), out _);
                        }
                    };
                Resources.Dialogues.GetDialogue(FeatureName.Shrine, ShrineDialogueName.Smintheus_Follow)
                    .Triggered += (t, eh) => {
                        foreach (var player in eh.DialogueListeners.Players()) {
                            var friends = Enumerable.Range(5, 10)
                                .Select(i => Resources.Entities
                                    .Rat(MonsterTierName.Two)
                                    .WithFaction(FactionName.Players)
                                    .WithPhysics(player.Physics.Position)
                                    .Build());
                            foreach (var f in friends) {
                                TrySpawn(player.FloorId(), f);
                            }
                        }
                    // Remove trigger from the shrine
                    if (Entities.TryGetFirstComponent<DialogueComponent>(eh.DialogueStarter.Id, out var dialogue)) {
                            dialogue.Triggers.Remove(t);
                        }
                    };
            }
        }

        /// <summary>
        /// Registers handlers for all system events. Is responsible for presentation-level business logic.
        /// </summary>
        /// <returns>A list of subscriptions that are automatically released when the scene goes into an exit state.</returns>
        public override IEnumerable<Subscription> RouteEvents()
        {
            // FloorSystem.FeatureRemoved:
                // - Mark feature for deletion
            yield return Systems.Floor.FeatureRemoved.SubscribeHandler(e => {
                Entities.FlagEntityForRemoval(e.OldState.Id);
            });
            // FloorSystem.TileRemoved:
                // - Mark tile for deletion
            yield return Systems.Floor.TileChanged.SubscribeHandler(e => {
                Entities.FlagEntityForRemoval(e.OldState.Id);
            });
            // ActionSystem.GameStarted:
                // - Clear old entities and references if present
                // - Generate a new RNG seed
                // - Generate map
                // - Create and spawn player
                // - Set faction relationships to default values
                // - Track player visually in the interface
            yield return Systems.Action.GameStarted.SubscribeResponse(e => {
                Systems.Floor.Reset();
                Entities.Clear(true);

                var newRngSeed = (int)DateTime.Now.ToBinary();
                Store.SetValue(Data.Global.RngSeed, newRngSeed);

                // Create player
                var playerName = Store.GetOrDefault(Data.Player.Name, "Player");
                Player = Resources.Entities.Player
                    .WithName(playerName)
                    .WithItems(Resources.Entities.Bow().Build(), Resources.Entities.Potion(PotionEffectName.Healing).Build())
                    .Build();

                // Generate map
                var entranceFloorId = new FloorId(DungeonBranchName.Dungeon, 1);
                Systems.Floor.AddDungeon(d => d.WithStep(ctx => {
                    ctx.AddBranch<DungeonBranchGenerator>(DungeonBranchName.Dungeon, 2, i => new Coord(100, 100));
                    // Connect branches at semi-random depths
                    ctx.Connect(default, entranceFloorId);
                }));

                var features = Systems.Floor.GetAllFeatures(entranceFloorId);
                Player.Physics.Position = features
                    .Single(t => t.FeatureProperties.Name == FeatureName.Upstairs)
                    .Physics.Position;

                if (!TrySpawn(entranceFloorId, Player)) {
                    throw new InvalidOperationException("Can't spawn the player??");
                }

                // Spawn all actors once at floorgen
                foreach (var comp in Entities.GetComponents<ActorComponent>()) {
                    var proxy = Entities.GetProxy<Actor>(comp.EntityId);
                    Systems.Action.Spawn(proxy);
                }

                // Track all actors on the first floor since the player's floorId was null during floorgen
                // Afterwards, this is handled when the player uses a portal or stairs or when a monster spawns
                foreach (var actor in Systems.Floor.GetAllActors(entranceFloorId)) {
                    Systems.Action.Track(actor.Id);
                }

                // Set faction defaults
                Systems.Faction.SetDefaultRelationships();
                Systems.Floor.RecalculateFov(Player);
                Systems.Render.CenterOn(Player);
                return true;
            });
            // ActionSystem.ActorTurnStarted:
                // - Update Fov
                // - Recenter viewport on player and update UI
            yield return Systems.Action.ActorTurnStarted.SubscribeHandler(e => {
                Systems.Floor.RecalculateFov(e.Actor);
                if (e.Actor.IsPlayer()) {
                    Systems.Render.CenterOn(e.Actor);
                }
            });
            // ActionSystem.ActorIntentEvaluated:
            yield return Systems.Action.ActorIntentEvaluated.SubscribeHandler(e => {
            });
            // ActionSystem.ActorTurnEnded:
                // - Check dialogue triggers when the player's turn ends
            yield return Systems.Action.ActorTurnEnded.SubscribeResponse(e => {
                if (e.Actor.IsPlayer()) {
                    Systems.Dialogue.CheckTriggers();
                }
                return true;
            });
            // ActionSystem.ActorMoved:
                // - Update actor position
                // - Update FloorSystem positional caches
                // - Log stuff that was stepped over
            yield return Systems.Action.ActorMoved.SubscribeResponse(e => {
                var floor = Systems.Floor.GetFloor(e.Actor.FloorId());
                floor.Cells[e.OldPosition].Actors.Remove(e.Actor);
                floor.Cells[e.NewPosition].Actors.Add(e.Actor);
                var itemsHere = Systems.Floor.GetItemsAt(floor.Id, e.NewPosition);
                var featuresHere = Systems.Floor.GetFeaturesAt(floor.Id, e.NewPosition);
                foreach (var item in itemsHere) {
                    e.Actor.Log?.Write($"$Action.YouStepOverA$ {item.DisplayName}.");
                }
                foreach (var feature in featuresHere) {
                    e.Actor.Log?.Write($"$Action.YouStepOverA$ {feature.Info.Name}.");
                }
                e.Actor.Physics.Position = e.NewPosition;
                return true;
            });
            // ActionSystem.ActorAttacked:
                // - Calculate damage depending on equipment 
                // - Handle Ai aggro and grudges
                // - Show projectile animations
            yield return Systems.Action.ActorAttacked.SubscribeResponse(e => {
                var swingDelay = 0;
                e.Attacker.Log?.Write($"$Action.YouAttack$ {e.Victim.Info.Name}.");
                e.Victim.Log?.Write($"{e.Attacker.Info.Name} $Action.AttacksYou$.");
                // animate projectile if this was a ranged attack
                if(e.Type == AttackName.Ranged) {
                    var dir = (e.Victim.Physics.Position - e.Attacker.Physics.Position).Clamp(-1, 1);
                    Systems.Render.Animate(
                        e.Attacker.Physics.Position,
                        Animation.Projectile(e.Attacker.Physics.Position, e.Victim.Physics.Position, tint: ColorName.Red)
                    );
                }
                // calculate damage
                var damage = 0;
                var weaponsUsed = e.Attacker.Equipment?.GetEquipedWeapons(w => w.AttackType == e.Type)
                    ?? Enumerable.Empty<Weapon>();
                if(!weaponsUsed.Any()) {
                    // Attack using intrinsics
                    damage = 1;
                }
                else {
                    // Attack using equipment
                    swingDelay = weaponsUsed.Max(w => w.WeaponProperties.SwingDelay);
                    damage = weaponsUsed.Sum(w => w.WeaponProperties.BaseDamage);
                }
                e.Victim.ActorProperties.Stats.Health -= damage;

                if(damage > 0) {
                    // make sure that neutrals aggro the attacker
                    if (e.Victim.Ai != null && e.Victim.Ai.Target == null) {
                        e.Victim.Ai.Target = e.Attacker;
                    }
                    // make sure that people hold a grudge regardless of factions
                    e.Victim.ActorProperties.Relationships.Update(e.Attacker, x => x
                        .With(StandingName.Hated)
                    , out _);
                }

                return new(damage, swingDelay, true);
            });
            // ActionSystem.ActorDespawned:
                // - Remove entity from floor and action systems and handle cleanup
            yield return Systems.Action.ActorDespawned.SubscribeResponse(e => {
                Systems.Action.StopTracking(e.Actor.Id);
                Systems.Floor.RemoveActor(e.Actor.FloorId(), e.Actor);
                Entities.FlagEntityForRemoval(e.Actor.Id);
                // e.Actor.TryRefresh(0); // invalidate target proxy
                return true;
            });
            // ActionSystem.ActorDied:
                // - Handle game over when the player dies
                // - Raise ActionSystem.ActorDespawned
            yield return Systems.Action.ActorDied.SubscribeResponse(e => {
                e.Actor.Log?.Write($"$Action.YouDie$.");
                if (e.Actor.IsPlayer()) {
                    Resources.Sounds.Get(SoundName.PlayerDeath).Play();
                }
                Systems.Action.ActorDespawned.Raise(new(e.Actor));
                return true;
            });
            // ActionSystem.ActorKilled:
                // - Raise ActionSystem.ActorDied
            yield return Systems.Action.ActorKilled.SubscribeResponse(e => {
                e.Victim.Log?.Write($"{e.Killer.Info.Name} $Action.KillsYou$.");
                e.Killer.Log?.Write($"$Action.YouKill$ {e.Victim.Info.Name}.");
                Systems.Action.ActorDied.Raise(new(e.Victim));
                return true;
            });
            // ActionSystem.ItemDropped:
                // - Drop item (remove from actor's inventory and add to floor) or fail if there's no space
            yield return Systems.Action.ItemDropped.SubscribeResponse(e => {
                if (Systems.Floor.TryGetClosestFreeTile(e.Actor.FloorId(), e.Actor.Physics.Position, out var tile)) {
                    if(!e.Actor.Inventory.TryTake(e.Item)) {
                        e.Actor.Log?.Write($"$Action.UnableToDrop$ {e.Item.DisplayName}.");
                        return false;
                    }
                    else {
                        e.Item.Physics.Position = tile.Physics.Position;
                        Systems.Floor.AddItem(e.Actor.FloorId(), e.Item);
                        e.Actor.Log?.Write($"$Action.YouDrop$ {e.Item.DisplayName}.");
                    }
                }
                else {
                    e.Actor.Log?.Write($"$Action.NoSpaceToDrop$ {e.Item.DisplayName}.");
                    return false;
                }
                return true;
            });
            // ActionSystem.ItemPickedUp:
                // - Store item in inventory or fail
            yield return Systems.Action.ItemPickedUp.SubscribeResponse(e => {
                if (e.Actor.Inventory.TryPut(e.Item)) {
                    Systems.Floor.RemoveItem(e.Actor.FloorId(), e.Item);
                    e.Actor.Log?.Write($"$Action.YouPickUpA$ {e.Item.DisplayName}.");
                    return true;
                }
                else {
                    e.Actor.Log?.Write($"$Action.YourInventoryIsTooFullFor$ {e.Item.DisplayName}.");
                    return false;
                }
            });
            // ActionSystem.ItemEquipped:
                // - Equip item or fail
            yield return Systems.Action.ItemEquipped.SubscribeResponse(e => {
                if (e.Actor.Equipment.TryEquip(e.Item)) {
                    e.Actor.Log?.Write($"$Action.YouEquip$ {e.Item.DisplayName}.");
                    return true;
                }
                else {
                    e.Actor.Log?.Write($"$Action.YouFailEquipping$ {e.Item.DisplayName}.");
                    return false;
                }
            });
            // ActionSystem.ItemUnequipped:
                // - Unequip item or fail
            yield return Systems.Action.ItemUnequipped.SubscribeResponse(e => {
                if (e.Actor.Equipment.TryUnequip(e.Item)) {
                    e.Actor.Log?.Write($"$Action.YouUnequip$ {e.Item.DisplayName}.");
                    return true;
                }
                else {
                    e.Actor.Log?.Write($"$Action.YouFailUnequipping$ {e.Item.DisplayName}.");
                    return false;
                }
            });
            // ActionSystem.ItemConsumed:
                // - Use up item and remove it from the floor when completely consumed
            yield return Systems.Action.ItemConsumed.SubscribeResponse(e => {
                if (TryUseItem(e.Item, e.Actor, out var consumed)) {
                    e.Actor.Log?.Write($"$Action.YouUse$ {e.Item.DisplayName}.");
                    if (consumed) {
                        e.Actor.Log?.Write($"$Action.AnItemIsConsumed$ {e.Item.DisplayName}.");
                    }
                    return true;
                }
                else {
                    e.Actor.Log?.Write($"$Action.YouFailUsing$ {e.Item.DisplayName}.");
                    return false;
                }
            });
            // ActionSystem.ActorBumpedObstacle:
                // - Play a sound when it's the player
            yield return Systems.Action.ActorBumpedObstacle.SubscribeHandler(e => {
                e.Actor.Log?.Write($"$Action.YouBumpInto$ {e.Obstacle.Info.Name}.");
                if(e.Actor.IsPlayer()) {
                    Resources.Sounds.Get(SoundName.WallBump, e.Obstacle.Physics.Position).Play();
                }
            });
            // ActionSystem.ActorSteppedOnTrap:
                // - Show an animation if the player can see this
            yield return Systems.Action.ActorSteppedOnTrap.SubscribeHandler(e => {
                e.Actor.Log?.Write($"$Action.YouTriggerATrap$.");
                if(Player.CanSee(e.Actor)) {
                    Systems.Render.Animate(e.Feature.Physics.Position, Animation.ExpandingRing(5, tint: ColorName.LightBlue));
                }
            });
            // ActionSystem.FeatureInteractedWith:
                // - Open/close doors
                // - Handle shrine interactions
                // - Handle chest interactions
                // - Handle stair and portal interactions
                    // - Empty and fill action queue on player floor change
            yield return Systems.Action.FeatureInteractedWith.SubscribeResponse(e => {
                if (e.Feature.FeatureProperties.Name == FeatureName.Door) {
                    e.Feature.Physics.BlocksMovement ^= true;
                    e.Feature.Physics.BlocksLight = e.Feature.Physics.BlocksMovement;
                    e.Feature.Render.Hidden = !e.Feature.Physics.BlocksMovement;
                    if (e.Feature.Physics.BlocksMovement) {
                        e.Actor.Log?.Write($"$Action.YouCloseThe$ {e.Feature.Info.Name}.");
                    }
                    else {
                        e.Actor.Log?.Write($"$Action.YouOpenThe$ {e.Feature.Info.Name}.");
                    }
                    return true;
                }
                if (e.Feature.FeatureProperties.Name == FeatureName.Shrine) {
                    e.Actor.Log?.Write($"$Action.YouKneelAt$ {e.Feature.Info.Name}.");
                    return true;
                }
                if (e.Feature.FeatureProperties.Name == FeatureName.Chest) {
                    e.Actor.Log?.Write($"$Action.YouOpenThe$ {e.Feature.Info.Name}.");
                    return true;
                }
                if (e.Feature.FeatureProperties.Name == FeatureName.Upstairs) {
                    return HandleStairs(e.Feature.Portal.Connection.To, e.Feature.Portal.Connection.From);
                }
                if (e.Feature.FeatureProperties.Name == FeatureName.Downstairs) {
                    return HandleStairs(e.Feature.Portal.Connection.From, e.Feature.Portal.Connection.To);
                }
                return false;
                bool HandleStairs(FloorId current, FloorId next)
                {
                    var currentFloor = Systems.Floor.GetFloor(current);
                    if (!Systems.Floor.TryGetFloor(next, out var nextFloor)) {
                        e.Actor.Log?.Write($"$Error.NoFloorWithId$ {next}.");
                        return false;
                    }
                    var stairs = Systems.Floor.GetAllFeatures(next)
                        .Single(f => f.Portal?.Connects(current, next) ?? false);
                    currentFloor.RemoveActor(e.Actor);
                    e.Actor.Physics.Position = stairs.Physics.Position;
                    e.Actor.Physics.FloorId = next;
                    nextFloor.AddActor(e.Actor);
                    e.Actor.Log?.Write($"$Action.YouTakeTheStairsTo$ {next}.");

                    if(e.Actor.IsPlayer()) {
                        foreach (var actor in Systems.Floor.GetAllActors(current)) {
                            Systems.Action.StopTracking(actor.Id);
                        }
                        foreach (var actor in Systems.Floor.GetAllActors(next)) {
                            Systems.Action.Track(actor.Id);
                        }
                        Systems.Action.AbortCurrentTurn();
                    }

                    return true;
                }
            });
        }

        public bool TrySpawn(FloorId floorId, Actor actor, float maxDistance = 10)
        {
            if(!Systems.Floor.TryGetClosestFreeTile(floorId, actor.Physics.Position, out var spawnTile, maxDistance)) {
                return false;
            }
            actor.Physics.Position = spawnTile.Physics.Position;
            Systems.Action.Track(actor.Id);
            Systems.Action.Spawn(actor);
            Systems.Floor.AddActor(floorId, actor);
            return true;
        }

        public bool TryUseItem(Item item, Actor actor, out bool consumed)
        {
            var used = false;
            consumed = false;
            if (item.TryCast<Consumable>(out var consumable)) {
                used = TryConsume(out consumed);
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
        }

        public override void Update()
        {
            Systems.Action.Update(Player.Id);
            Systems.Render.Update();
            if (Input.IsKeyPressed(Key.R)) {
                TrySetState(SceneState.Main);
            }
        }

        public override void Draw()
        {
            UI.Window.Clear();
            Systems.Render.Draw();
        }

        protected override bool CanChangeState(SceneState newState) => true;
        protected override void OnStateChanged(SceneState oldState)
        {
            base.OnStateChanged(oldState);
            if (State == SceneState.Main) {
                Systems.Action.Reset();
            }
        }
    }
}
