using Fiero.Core;
using Microsoft.VisualBasic;
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
        protected readonly GameEntityBuilders EntityBuilders;
        protected readonly OffButton OffButton;
        protected readonly GameUI UI;

        public Actor Player { get; private set; }

        public GameplayScene(
            GameInput input,
            GameDataStore store,
            GameEntities entities,
            GameSystems systems,
            GameResources resources,
            GameEntityBuilders entityBuilders,
            GameUI ui,
            OffButton off)
        {
            Input = input;
            Store = store;
            Entities = entities;
            Systems = systems;
            EntityBuilders = entityBuilders;
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
                            Systems.Faction.SetBilateralRelationship(FactionName.Rats, FactionName.Players, StandingName.Loved);
                        }
                    };
                Resources.Dialogues.GetDialogue(NpcName.GreatKingRat, GKRDialogueName.JustMet_Enemy)
                    .Triggered += (t, eh) => {
                        var gkr = (Actor)eh.DialogueStarter;
                        foreach (var player in eh.DialogueListeners.Players()) {
                            Systems.Faction.SetBilateralRelationship(FactionName.Rats, FactionName.Players, StandingName.Hated);
                            Systems.Faction.SetBilateralRelationship(player, gkr, StandingName.Hated);
                        }
                    };
                Resources.Dialogues.GetDialogue(FeatureName.Shrine, ShrineDialogueName.Smintheus_Follow)
                    .Triggered += (t, eh) => {
                        foreach (var player in eh.DialogueListeners.Players()) {
                            var friends = Enumerable.Range(5, 10)
                                .Select(i => Resources.Entities
                                    .NPC_Rat()
                                    .WithPhysics(player.Position())
                                    .Build());
                            foreach (var f in friends) {
                                if(TrySpawn(player.FloorId(), f)) {
                                    Systems.Faction.SetBilateralRelationship(player, f, StandingName.Loved);
                                }
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
                    .WithHealth(100)
                    .WithItems(
                        Resources.Entities.Weapon_Sword().Build(), 
                        Resources.Entities.Potion(EffectName.Confusion).Build(),
                        Resources.Entities.Throwable_Rock(10).Build())
                    //.WithSpells(
                    //    Resources.Entities.Spell_CrimsonLance().Build(),
                    //    Resources.Entities.Spell_Bloodbath().Build(),
                    //    Resources.Entities.Spell_ClotBlock().Build())
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
                    .Position();

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
                Systems.Render.Screen.CenterOn(Player);
                return true;
            });
            // ActionSystem.ActorIntentFailed:
                // - Repaint viewport if actor
            yield return Systems.Action.ActorIntentFailed.SubscribeHandler(e => {
                if(e.Actor.IsPlayer()) {
                    Systems.Render.Screen.CenterOn(e.Actor);
                }
            });
            // ActionSystem.ActorTurnStarted:
                // - Update Fov
                // - Attempt to auto-identify items that can be seen
                // - Recenter viewport on player and update UI
            yield return Systems.Action.ActorTurnStarted.SubscribeHandler(e => {
                var floorId = e.Actor.FloorId();
                Systems.Floor.RecalculateFov(e.Actor);
                foreach (var p in e.Actor.Fov.VisibleTiles[floorId]) {
                    foreach (var item in Systems.Floor.GetItemsAt(floorId, p)) {
                        e.Actor.TryIdentify(item);
                    }
                }
                if (e.Actor.IsPlayer()) {
                    Systems.Render.Screen.CenterOn(e.Actor);
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
                foreach (var items in itemsHere.GroupBy(i => i.DisplayName)) {
                    var count = items.Count();
                    if(count == 1) {
                        e.Actor.Log?.Write($"$Action.YouStepOverA$ {items.Key}.");
                    }
                    else {
                        e.Actor.Log?.Write($"$Action.YouStepOverSeveral$ {count} {items.Key}.");
                    }
                }
                foreach (var features in featuresHere.GroupBy(i => i.FeatureProperties.Name)) {
                    var count = features.Count();
                    if (count == 1) {
                        e.Actor.Log?.Write($"$Action.YouStepOverA$ {features.Key}.");
                    }
                    else {
                        e.Actor.Log?.Write($"$Action.YouStepOverSeveral$ {count} {features.Key}.");
                    }
                }
                e.Actor.Physics.Position = e.NewPosition;
                return true;
            });
            // ActionSystem.ActorAttacked:
                // - Handle Ai aggro and grudges
                // - Show melee attack animation
            yield return Systems.Action.ActorAttacked.SubscribeResponse(e => {
                e.Attacker.Log?.Write($"$Action.YouAttack$ {e.Victim.Info.Name}.");
                e.Victim.Log?.Write($"{e.Attacker.Info.Name} $Action.AttacksYou$.");
                var dir = (e.Victim.Position() - e.Attacker.Position()).Clamp(-1, 1);
                if (e.Type == AttackName.Melee) {
                    Resources.Sounds.Get(SoundName.MeleeAttack, e.Attacker.Position() - Player.Position()).Play();
                    if (Player.CanSee(e.Attacker)) {
                        e.Attacker.Render.Hidden = true;
                        Systems.Render.Screen.CenterOn(Player);
                        Systems.Render.Screen.Animate(true, e.Attacker.Position(), Animation.MeleeAttack(e.Attacker, dir));
                        e.Attacker.Render.Hidden = false;
                    }
                }
                return true;
            });
            // ActionSystem.ActorDamaged 
                // - Deal damage
                // - Handle aggro
                // - Spawn blood splatters
            yield return Systems.Action.ActorDamaged.SubscribeResponse(e => {
                e.Victim.ActorProperties.Stats.Health -= e.Damage;
                if (e.Damage > 0) {
                    if (e.Source.TryCast<Actor>(out var attacker)) {
                        // make sure that neutrals aggro the attacker
                        if (e.Victim.Ai != null && e.Victim.Ai.Target == null) {
                            e.Victim.Ai.Target = attacker;
                        }
                        // make sure that people hold a grudge regardless of factions
                        Systems.Faction.SetUnilateralRelationship(e.Victim, attacker, StandingName.Hated);
                    }
                }
                Systems.Render.Screen.Animate(false, e.Victim.Position(), Animation.DamageNumber(e.Damage,
                    e.Victim.IsPlayer() ? ColorName.LightRed : ColorName.LightCyan));
                return true;
            });
            // ActionSystem.ActorDespawned:
                // - Handle game over when the player dies
                // - Remove entity from floor and action systems and handle cleanup
            yield return Systems.Action.ActorDespawned.SubscribeResponse(e => {
                if (e.Actor.IsPlayer()) {
                    TrySetState(SceneState.Main);
                }
                else {
                    Systems.Action.StopTracking(e.Actor.Id);
                    Systems.Floor.RemoveActor(e.Actor);
                    Entities.FlagEntityForRemoval(e.Actor.Id);
                }
                return true;
            });
            // ActionSystem.ActorDied:
                // - Play death animation
            yield return Systems.Action.ActorDied.SubscribeResponse(e => {
                e.Actor.Log?.Write($"$Action.YouDie$.");
                if (e.Actor.IsPlayer()) {
                    Resources.Sounds.Get(SoundName.PlayerDeath).Play();
                }
                else {
                    Resources.Sounds.Get(SoundName.EnemyDeath, e.Actor.Position() - Player.Position()).Play();
                }
                e.Actor.Render.Hidden = true;
                if(Player.CanSee(e.Actor)) {
                    // Since this is a blocking animation and we just hid the victim, we need to refresh the viewport before showing it
                    Systems.Render.Screen.SetDirty();
                    Systems.Render.Screen.Animate(true, e.Actor.Position(), Animation.Death(e.Actor));
                }
                return true;
            });
            // ActionSystem.ActorKilled:
            yield return Systems.Action.ActorKilled.SubscribeResponse(e => {
                Systems.Render.Screen.Animate(false, e.Victim.Position(), Animation.Explosion());
                e.Victim.Log?.Write($"{e.Killer.Info.Name} $Action.KillsYou$.");
                e.Killer.Log?.Write($"$Action.YouKill$ {e.Victim.Info.Name}.");
                return true;
            });
            // ActionSystem.ItemDropped:
                // - Drop item (remove from actor's inventory and add to floor) or fail if there's no space
            yield return Systems.Action.ItemDropped.SubscribeResponse(e => {
                if (Systems.Floor.TryGetClosestFreeTile(e.Actor.FloorId(), e.Actor.Position(), out var tile, 10,
                        cell => !cell.Items.Any() && !cell.Features.Any())) {
                    if(!e.Actor.Inventory.TryTake(e.Item)) {
                        e.Actor.Log?.Write($"$Action.UnableToDrop$ {e.Item.DisplayName}.");
                        return false;
                    }
                    else {
                        e.Item.Physics.Position = tile.Position();
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
                // - Play a sound if it's the player
            yield return Systems.Action.ItemPickedUp.SubscribeResponse(e => {
                if (e.Actor.Inventory.TryPut(e.Item, out var fullyMerged)) {
                    Systems.Floor.RemoveItem(e.Item);
                    if(fullyMerged) {
                        Entities.FlagEntityForRemoval(e.Item.Id);
                    }
                    e.Actor.Log?.Write($"$Action.YouPickUpA$ {e.Item.DisplayName}.");
                    if(e.Actor.IsPlayer()) {
                        Resources.Sounds.Get(SoundName.ItemPickedUp).Play();
                    }
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
            // ActionSystem.ItemThrown:
                // - "Use" item in order to consume its charges
                // - Spawn a 1-charge item where the consumable lands if it doesn't mulch
                // - Play an animation and a sound as the projectile flies
            yield return Systems.Action.ItemThrown.SubscribeResponse(e => {
                if (TryUseItem(e.Item, e.Actor, out var consumed)) {
                    e.Actor.Log?.Write($"$Action.YouThrow$ {e.Item.DisplayName}.");
                    Resources.Sounds.Get(SoundName.RangedAttack, e.Actor.Position() - Player.Position()).Play();
                    if (Player.CanSee(e.Actor) || Player.CanSee(e.Victim)) {
                        var anim = Animation.ArcingProjectile(e.Actor.Position(), e.Position, sprite: e.Item.Render.SpriteName);
                        Systems.Render.Screen.Animate(true, e.Actor.Position(), anim);
                        Resources.Sounds.Get(SoundName.MeleeAttack, e.Position - Player.Position()).Play();
                    }
                    if (Rng.Random.NextDouble() >= e.Item.ThrowableProperties.MulchChance) {
                        var clone = (Throwable)e.Item.Clone();
                        clone.ConsumableProperties.RemainingUses = 1;
                        clone.Physics.Position = e.Position;
                        Systems.Floor.AddItem(e.Actor.FloorId(), clone);
                    }
                    else {
                        Systems.Render.Screen.Animate(false, e.Position, Animation.Explosion(scale: new(0.5f, 0.5f)));
                    }
                    return true;
                }
                else {
                    e.Actor.Log?.Write($"$Action.YouFailThrowing$ {e.Item.DisplayName}.");
                    return false;
                }
            });
            // ActionSystem.ItemConsumed:
                // - Use up item and remove it from the floor when completely consumed
                // - Identify potions and scrolls (not wands, because they need to hit something)
            yield return Systems.Action.ItemConsumed.SubscribeResponse(e => {
                if (TryUseItem(e.Item, e.Actor, out var consumed)) {
                    e.Actor.Log?.Write($"$Action.YouUse$ {e.Item.DisplayName}.");
                    if(e.Item.TryCast<Potion>(out var p) && e.Actor.Identify(p, q => q.PotionProperties.Name == p.PotionProperties.Name)) {
                        e.Actor.Log?.Write($"$Action.YouIdentifyAPotion$ {e.Item.DisplayName}.");
                    }
                    if (e.Item.TryCast<Scroll>(out var s) && e.Actor.Identify(s, t => t.ScrollProperties.Name == s.ScrollProperties.Name)) {
                        e.Actor.Log?.Write($"$Action.YouIdentifyAScroll$ {e.Item.DisplayName}.");
                    }
                    Resources.Sounds.Get(SoundName.ItemUsed, e.Actor.Position() - Player.Position()).Play();
                    if (Player.CanSee(e.Actor)) {
                        Systems.Render.Screen.Animate(false, e.Actor.Position(), Animation.ExpandingRing(5, tint: ColorName.LightMagenta));
                    }
                    return true;
                }
                else {
                    e.Actor.Log?.Write($"$Action.YouFailUsing$ {e.Item.DisplayName}.");
                    return false;
                }
            });
            // ActionSystem.SpellLearned:
                // - Add spell to actor's library
                // - Play sound and animation if player
            yield return Systems.Action.SpellLearned.SubscribeResponse(e => {
                if (!e.Actor.Spells.Learn(e.Spell))
                    return false;
                e.Actor.Log?.Write($"$Action.YouLearn$ {e.Spell.Info.Name}.");
                if(e.Actor.IsPlayer()) {
                    // TODO: Play sound and animation if player
                }
                return true;
            });
            // ActionSystem.SpellForgotten:
                // - Remove spell from actor's library
                // - Play sound and animation if player
            yield return Systems.Action.SpellForgotten.SubscribeResponse(e => {
                if (!e.Actor.Spells.Forget(e.Spell))
                    return false;
                e.Actor.Log?.Write($"$Action.YouForget$ {e.Spell.Info.Name}.");
                if (e.Actor.IsPlayer()) {
                    // TODO: Play sound and animation if player
                }
                return true;
            });
            // ActionSystem.SpellTargeted:
                // - Check effect preconditions
            yield return Systems.Action.SpellTargeted.SubscribeResponse(e => {
                foreach (var effect in e.Spell.Effects.Active) {
                    if(effect is CastEffect cast && !cast.ShouldApply(Systems, e.Spell, e.Actor)) {
                        return false;
                    }
                }
                return true;
            });
            // ActionSystem.SpellCast:
            yield return Systems.Action.SpellCast.SubscribeResponse(e => {
                return true;
            });
            // ActionSystem.ActorBumpedObstacle:
                // - Play a sound when it's the player
            yield return Systems.Action.ActorBumpedObstacle.SubscribeHandler(e => {
                if(e.Obstacle != null) {
                    e.Actor.Log?.Write($"$Action.YouBumpInto$ {e.Obstacle.Info.Name}.");
                    if (e.Actor.IsPlayer()) {
                        Resources.Sounds.Get(SoundName.WallBump, e.Obstacle.Position() - Player.Position()).Play();
                    }
                }
                else {
                    e.Actor.Log?.Write($"$Action.YouBumpIntoTheVoid$.");
                }
            });
            // ActionSystem.ActorSteppedOnTrap:
                // - Show an animation if the player can see this
            yield return Systems.Action.ActorSteppedOnTrap.SubscribeHandler(e => {
                e.Actor.Log?.Write($"$Action.YouTriggerATrap$.");
                if(Player.CanSee(e.Actor)) {
                    var pos = e.Feature.Position();
                    Resources.Sounds.Get(SoundName.TrapSpotted, pos - Player.Position()).Play();
                    Systems.Render.Screen.Animate(false, pos, Animation.ExpandingRing(5, tint: ColorName.LightBlue));
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
                if(e.Feature.TryCast<Portal>(out var portal)) {
                    if (e.Feature.FeatureProperties.Name == FeatureName.Upstairs) {
                        return HandleStairs(portal.PortalProperties.Connection.To, portal.PortalProperties.Connection.From);
                    }
                    if (e.Feature.FeatureProperties.Name == FeatureName.Downstairs) {
                        return HandleStairs(portal.PortalProperties.Connection.From, portal.PortalProperties.Connection.To);
                    }
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
                        .TrySelect(f => (f.TryCast<Portal>(out var portal), portal))
                        .Single(f => f.PortalProperties.Connects(current, next));
                    currentFloor.RemoveActor(e.Actor);
                    e.Actor.Physics.Position = stairs.Position();
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
                        // Abruptly stop the current turn so that the actor queue is flushed completely
                        Systems.Action.AbortCurrentTurn();
                    }

                    return true;
                }
            });
        }

        public bool TrySpawn(FloorId floorId, Actor actor, float maxDistance = 10)
        {
            if(!Systems.Floor.TryGetClosestFreeTile(floorId, actor.Position(), out var spawnTile, maxDistance)) {
                return false;
            }
            actor.Physics.Position = spawnTile.Position();
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
