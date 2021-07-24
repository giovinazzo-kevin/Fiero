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
                                    .WithPosition(player.Physics.Position)
                                    .Build());
                            foreach (var f in friends) {
                                TrySpawn(player.ActorProperties.FloorId, f);
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
            // ActionSystem.GameStarted:
                // - Clear old entities and references if present
                // - Generate a new RNG seed
                // - Generate map and spawn actors
                // - Create and spawn player
                // - Set faction relationships to default values
                // - Track player visually in the interface
            yield return Systems.Action.GameStarted.SubscribeResponse(e => {
                Systems.Floor.Reset();
                Entities.Clear(true);

                var newRngSeed = (int)DateTime.Now.ToBinary();
                Store.SetValue(Data.Global.RngSeed, newRngSeed);

                // Generate map
                var entranceFloorId = new FloorId(DungeonBranchName.Dungeon, 1);
                Systems.Floor.AddDungeon(d => d.WithStep(ctx => {
                    ctx.AddBranch<DungeonBranchGenerator>(DungeonBranchName.Dungeon, 2, i => new Coord(100, 100));
                    // Connect branches at semi-random depths
                    ctx.Connect(default, entranceFloorId);
                }));

                // Track agents
                foreach (var comp in Entities.GetComponents<ActionComponent>()) {
                    Systems.Action.AddActor(comp.EntityId);
                }

                // Create player on top of the starting stairs
                var playerName = Store.GetOrDefault(Data.Player.Name, "Player");
                var upstairs = Systems.Floor.GetAllFeatures(entranceFloorId)
                    .Single(t => t.FeatureProperties.Name == FeatureName.Upstairs)
                    .Physics.Position;
                Player = Resources.Entities.Player
                    .WithPosition(upstairs)
                    .WithName(playerName)
                    .WithItems(Resources.Entities.Bow().Build())
                    .Build();
                if (!TrySpawn(entranceFloorId, Player)) {
                    throw new InvalidOperationException("Can't spawn the player??");
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
                if (e.Actor.ActorProperties.Type == ActorName.Player) {
                    Systems.Render.CenterOn(e.Actor);
                }
            });
            // ActionSystem.ActorIntentEvaluated:
            yield return Systems.Action.ActorIntentEvaluated.SubscribeHandler(e => {
            });
            // ActionSystem.ActorTurnEnded:
            // - Check dialogue triggers when the player's turn ends
            yield return Systems.Action.ActorTurnEnded.SubscribeResponse(e => {
                if (e.Actor.ActorProperties.Type == ActorName.Player) {
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
                var addCost = 0;
                e.Attacker.Log?.Write($"$Action.YouAttack$ {e.Victim.Info.Name}.");
                e.Victim.Log?.Write($"{e.Attacker.Info.Name} $Action.AttacksYou$.");
                // animate projectile if this was a ranged attack
                if(e.Type == AttackName.Ranged) {
                    var dir = (e.Victim.Physics.Position - e.Attacker.Physics.Position).Clamp(-1, 1);
                    Systems.Render.Animate(
                        e.Attacker.Physics.Position,
                        Animation.Projectile(e.Attacker.Physics.Position, e.Victim.Physics.Position, tint: Color.Red)
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
                    addCost = weaponsUsed.Max(w => w.WeaponProperties.SwingDelay);
                    damage = weaponsUsed.Sum(w => w.WeaponProperties.BaseDamage);
                }
                e.Victim.ActorProperties.Health -= damage;

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

                return new(true, addCost);
            });
            // ActionSystem.ActorKilled:
                // - Remove entity from floor and handle cleanup
                // - Handle game over when the player dies
            yield return Systems.Action.ActorKilled.SubscribeResponse(e => {
                e.Victim.Log?.Write($"{e.Killer.Info.Name} $Action.KillsYou$.");
                e.Killer.Log?.Write($"$Action.YouKill$ {e.Victim.Info.Name}.");
                if (e.Victim.ActorProperties.Type == ActorName.Player) {
                    Resources.Sounds.Get(SoundName.PlayerDeath).Play();
                    Store.SetValue(Data.Player.KilledBy, e.Killer);
                }
                Systems.Floor.RemoveActor(e.Victim.ActorProperties.FloorId, e.Victim);
                Entities.FlagEntityForRemoval(e.Victim.Id);
                Entities.RemoveFlagged(true);
                e.Victim.TryRefresh(0); // invalidate target proxy
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
            // ActionSystem.FeatureInteractedWith:
                // - Open/close doors
                // - Handle shrine interactions
                // - Handle chest interactions
                // - Handle stair and portal interactions
            yield return Systems.Action.FeatureInteractedWith.SubscribeResponse(e => {
                if (e.Feature.FeatureProperties.Name == FeatureName.Door) {
                    e.Actor.Log?.Write($"$Action.YouOpenThe$ {e.Feature.Info.Name}.");
                    e.Feature.FeatureProperties.BlocksMovement ^= true;
                    e.Feature.FeatureProperties.BlocksLight = e.Feature.FeatureProperties.BlocksMovement;
                    e.Feature.Render.Hidden = !e.Feature.FeatureProperties.BlocksMovement;
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
                    e.Actor.ActorProperties.FloorId = next;
                    nextFloor.AddActor(e.Actor);
                    e.Actor.Log?.Write($"$Action.YouTakeTheStairsTo$ {next}.");
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
            Systems.Action.AddActor(actor.Id);
            Systems.Floor.AddActor(floorId, actor);
            return true;
        }

        public bool TryUseItem(Item item, Actor actor, out bool consumed)
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

        public override void Update()
        {
            Systems.Action.Update(Player.Id);
            Systems.Render.Update();
            Entities.RemoveFlagged();
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
