using SFML.Graphics;
using Unconcern.Common;

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

        protected readonly MetaSystem Systems;
        protected readonly GameResources Resources;
        protected readonly GameDataStore Store;
        protected readonly GameEntities Entities;
        protected readonly OffButton OffButton;
        protected readonly QuickSlotHelper QuickSlots;
        protected readonly GameUI UI;
        /// <summary>
        /// The action system is only updated once per frame to ensure that scripts don't stall the rendering.
        /// This flags gets reset after drawing the scene.
        /// NOTE: An action system update is not equal to one turn, but to one tick.
        /// </summary>
        private bool _newFrame = true;

        public Actor Player { get; private set; }

        public readonly IScriptHost<ScriptName> ScriptHost;

        public GameplayScene(
            GameDataStore store,
            GameEntities entities,
            MetaSystem systems,
            GameResources resources,
            QuickSlotHelper quickSlots,
            GameUI ui,
            OffButton off)
        {
            Store = store;
            Entities = entities;
            Systems = systems;
            Resources = resources;
            QuickSlots = quickSlots;
            UI = ui;
            OffButton = off;
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            GenerateNewRngSeed();
        }

        /// <summary>
        /// Registers handlers for all system events. Is responsible for presentation-level business logic.
        /// </summary>
        /// <returns>A list of subscriptions that are automatically released when the scene goes into an exit state.</returns>
        public override IEnumerable<Subscription> RouteEvents()
        {
            return RouteActionSystemEvents()
                .Concat(RouteFloorSystemEvents())
                .Concat(RouteRenderSystemEvents())
                .Concat(RouteScriptingSystemEvents())
                ;
        }

        private IEnumerable<Subscription> RouteRenderSystemEvents()
        {
            yield break;
        }
        private IEnumerable<Subscription> RouteFloorSystemEvents()
        {
            var dungeonSystem = Systems.Get<DungeonSystem>();
            // FloorSystem.FeatureRemoved:
            // - Mark feature for deletion
            yield return dungeonSystem.FeatureRemoved.SubscribeHandler(e =>
            {
                Entities.FlagEntityForRemoval(e.OldState.Id);
            });
            // FloorSystem.TileChanged:
            // - Mark old tile for deletion
            yield return dungeonSystem.TileChanged.SubscribeHandler(e =>
            {
                if (e.OldState.Id != e.NewState.Id)
                {
                    Entities.FlagEntityForRemoval(e.OldState.Id);
                }
            });
        }
        private IEnumerable<Subscription> RouteScriptingSystemEvents()
        {
            // Route all scripts
            foreach (var sub in Resources.Scripts.RouteSubscriptions())
                yield return sub;
            // Track the console output
            yield return Systems.Get<RenderSystem>().DeveloperConsole.TrackShell();
        }
        private IEnumerable<Subscription> RouteActionSystemEvents()
        {
            var actionSystem = Systems.Get<ActionSystem>();
            var dungeonSystem = Systems.Get<DungeonSystem>();
            var factionSystem = Systems.Get<FactionSystem>();
            var renderSystem = Systems.Get<RenderSystem>();
            var dialogueSystem = Systems.Get<DialogueSystem>();
            // ActionSystem.GameStarted:
            // - Clear old entities and references if present
            // - Clear scratch textures for procedural sprites
            // - Generate map
            // - Create and spawn player
            // - Set faction Relations to default values
            // - Track player visually in the interface
            yield return actionSystem.GameStarted.SubscribeResponse(e =>
            {
                dungeonSystem.Reset();
                Resources.Textures.ClearProceduralTextures();
                Resources.Sprites.ClearProceduralSprites();
                QuickSlots.UnsetAll();
                Entities.Clear(true);
                // Create player
                var playerName = Store.GetOrDefault(Data.Player.Name, "Player");
                Item[] loadout = Store.GetOrDefault(Data.Player.Loadout, LoadoutName.Adventurer) switch
                {
                    LoadoutName.Knight => [
                        EntityGenerator.GenerateMeleeWeapon(Resources.Entities).Build()
                    ],
                    LoadoutName.Archer => [Resources.Entities.Weapon_Bow().Build()],
                    LoadoutName.Wizard => [Resources.Entities.Wand_OfConfusion(charges: 25, duration: 5).Build()],
                    LoadoutName.Adventurer => [Resources.Entities.Projectile_Grapple().Build()],
                    LoadoutName.Merchant => [Resources.Entities.Resource_Gold(amount: 500).Build()],
                    LoadoutName.Warlock => [
                            Resources.Entities.Wand_OfPoison(charges: 25, duration: 1).Build(),
                        .. Enumerable.Range(0, 3).Select(_ => Resources.Entities.Scroll_OfRaiseUndead().Build()).ToArray()],
                    _ => []
                };

                Player = Resources.Entities.Player()
                    //.WithAutoPlayerAi()
                    .WithName(playerName)
                    .WithItems(loadout)
                    //.WithIntrinsicTrait(Traits.Invulnerable)
                    //.Tweak<PhysicsComponent>((s, c) => c.Phasing = true)
                    .WithHealth(10)
                    .Build();
                Player.TryJoinParty(Player);
                Store.SetValue(Data.Player.Id, Player.Id);
                // Generate map
                var entranceFloorId = new FloorId(DungeonBranchName.Dungeon, 1);
                dungeonSystem.AddDungeon(d => d.WithStep(ctx =>
                {
                    // BIG TODO: Once serialization is a thing, generate and load levels one at a time
                    ctx.AddBranch<EMLBranchGenerator>(DungeonBranchName.Dungeon, 3);
                    // Connect branches at semi-random depths
                    ctx.Connect(default, entranceFloorId);
                }));

                var spawnPoint = dungeonSystem.GetSpawnPoint(entranceFloorId);
                Player.Physics.Position = spawnPoint;

                if (!Systems.TrySpawn(entranceFloorId, Player, maxDistance: 100))
                {
                    throw new InvalidOperationException("Can't spawn the player??");
                }

                // Spawn all actors once at floorgen
                foreach (var comp in Entities.GetComponents<ActorComponent>()
                    .Except([Player.ActorProperties]))
                {
                    var proxy = Entities.GetProxy<Actor>(comp.EntityId);
                    actionSystem.Spawn(proxy);
                }

                // Track all actors on the first floor since the player's floorId was null during floorgen
                // Afterwards, this is handled when the player uses a portal or stairs or when a monster spawns
                foreach (var actor in dungeonSystem.GetAllActors(entranceFloorId))
                {
                    actionSystem.Track(actor.Id);
                }

                // Set faction defaults
                factionSystem.SetDefaultRelations();
                dungeonSystem.RecalculateFov(Player);
                renderSystem.CenterOn(Player);
                return true;
            });
            // ActionSystem.ActorIntentFailed:
            // - Repaint viewport if actor
            yield return actionSystem.ActorIntentFailed.SubscribeHandler(e =>
            {
                if (e.Actor.IsPlayer())
                {
                    renderSystem.CenterOn(e.Actor);
                }
            });
            // ActionSystem.ActorTurnStarted:
            // - Update Fov
            // - Attempt to auto-identify items that can be seen
            // - Recenter viewport on player and update UI
            // - Restore some HP if not in combat
            yield return actionSystem.ActorTurnStarted.SubscribeHandler(e =>
            {
                const int REST_DELAY = 5; // How many turns must pass after taking damage before regen starts

                var floorId = e.Actor.FloorId();
                dungeonSystem.RecalculateFov(e.Actor);
                foreach (var p in e.Actor.Fov.VisibleTiles[floorId])
                {
                    foreach (var item in dungeonSystem.GetItemsAt(floorId, p))
                    {
                        e.Actor.TryIdentify(item);
                    }
                }
                if (e.Actor.IsPlayer())
                {
                    renderSystem.CenterOn(e.Actor);
                }

                if (e.TurnId > e.Actor.ActorProperties.LastTookDamageOnTurn + REST_DELAY)
                    e.Actor.ActorProperties.Health.V++;
                // TODO: Make the delay configurable!
                if (e.Actor.Action.ActionProvider.RequestDelay)
                {
                    renderSystem.AnimateViewport(true, e.Actor.Location(), Animation.Wait(TimeSpan.FromMilliseconds(5)));
                }
            });
            // ActionSystem.ActorIntentEvaluated:
            // - Wait, if the action provider is asking for a delay
            yield return actionSystem.ActorIntentEvaluated.SubscribeHandler(e =>
            {
            });
            // ActionSystem.ActorTurnEnded:
            // - Check dialogue triggers when the player's turn ends
            yield return actionSystem.ActorTurnEnded.SubscribeResponse(e =>
            {
                if (e.Actor.IsPlayer())
                {
                    dialogueSystem.CheckTriggers();
                }
                return true;
            });
            // ActionSystem.ActorTeleported:
            // - Show animation and play sound
            yield return actionSystem.ActorTeleporting.SubscribeResponse(e =>
            {
                var floorId = e.Actor.FloorId();
                var (seeOld, seeNew) = (Player.CanSee(floorId, e.OldPosition), Player.CanSee(floorId, e.NewPosition));

                if (seeOld && !seeNew)
                {
                    TpOut();
                    actionSystem.ActorMoved.HandleOrThrow(e);
                    return true;
                }
                else if (!seeOld && (seeNew || e.Actor.IsPlayer()))
                {
                    actionSystem.ActorMoved.HandleOrThrow(e);
                    TpIn();
                    return true;
                }
                else if (seeOld && seeNew)
                {
                    TpOut();
                    actionSystem.ActorMoved.HandleOrThrow(e);
                    TpIn();
                    return true;
                }
                actionSystem.ActorMoved.HandleOrThrow(e);
                return true;

                void TpOut()
                {
                    renderSystem.CenterOn(Player);
                    var tpOut = Animation.TeleportOut(e.Actor)
                        .OnFirstFrame(() =>
                        {
                            if (!e.Actor.IsInvalid())
                                e.Actor.Render.Hidden = true;
                            renderSystem.CenterOn(Player);
                        })
                        .OnLastFrame(() =>
                        {
                            if (!e.Actor.IsInvalid())
                                e.Actor.Render.Hidden = false;
                        });
                    if (!e.Actor.IsPlayer())
                        Player.Log?.Write($"{e.Actor.Info.Name} $Action.TeleportsAway$.");
                    e.Actor.Log?.Write($"$Action.YouTeleportAway$.");
                    Resources.Sounds.Get(SoundName.SpellCast, e.OldPosition - Player.Position()).Play();
                    renderSystem.AnimateViewport(true, new Location(e.Actor.FloorId(), e.OldPosition), tpOut);
                }

                void TpIn()
                {
                    var tpIn = Animation.TeleportIn(e.Actor)
                        .OnFirstFrame(() =>
                        {
                            if (e.Actor.IsPlayer() || Player.CanSee(floorId, e.NewPosition))
                            {
                                if (!e.Actor.IsInvalid())
                                    e.Actor.Render.Hidden = true;
                                renderSystem.CenterOn(Player);
                            }
                        })
                        .OnLastFrame(() =>
                        {
                            if (e.Actor.IsInvalid()) return;
                            if (e.Actor.IsPlayer() || Player.CanSee(floorId, e.NewPosition))
                            {
                                if (!e.Actor.IsInvalid())
                                    e.Actor.Render.Hidden = false;
                                renderSystem.CenterOn(Player);
                            }
                        });
                    if (e.Actor.IsPlayer())
                    {
                        dungeonSystem.RecalculateFov(Player);
                        renderSystem.CenterOn(Player);
                    }
                    else if (Player.CanSee(e.Actor))
                    {
                        Player.Log.Write($"{e.Actor.Info.Name} $Action.TeleportsIn$.");
                    }
                    Resources.Sounds.Get(SoundName.SpellCast, e.NewPosition - Player.Position()).Play();
                    renderSystem.AnimateViewport(true, new Location(e.Actor.FloorId(), e.NewPosition), tpIn);
                }
            });
            // ActionSystem.ActorMoved:
            // - Update actor position
            // - Update FloorSystem positional caches
            // - Log stuff that was stepped over
            // - Play animation if enabled in the settings or if this is the AutoPlayer
            yield return actionSystem.ActorMoved.SubscribeResponse(e =>
            {
                if (e.Actor.IsInvalid())
                    return true;
                if (!dungeonSystem.TryGetFloor(e.Actor.FloorId(), out var floor)
                || !floor.Cells.ContainsKey(e.OldPosition)
                || !floor.Cells.ContainsKey(e.NewPosition))
                    return false;
                floor.Cells[e.OldPosition].Actors.Remove(e.Actor);
                floor.Cells[e.NewPosition].Actors.Add(e.Actor);
                var itemsHere = dungeonSystem.GetItemsAt(floor.Id, e.NewPosition);
                var featuresHere = dungeonSystem.GetFeaturesAt(floor.Id, e.NewPosition);
                foreach (var items in itemsHere.GroupBy(i => i.DisplayName))
                {
                    var count = items.Count();
                    if (count == 1)
                    {
                        e.Actor.Log?.Write($"$Action.YouStepOverA$ {items.Key}.");
                    }
                    else
                    {
                        e.Actor.Log?.Write($"$Action.YouStepOverSeveral$ {count} {items.Key}.");
                    }
                }
                foreach (var features in featuresHere.GroupBy(i => i.FeatureProperties.Name))
                {
                    var count = features.Count();
                    if (count == 1)
                    {
                        e.Actor.Log?.Write($"$Action.YouStepOverA$ {features.Key}.");
                    }
                    else
                    {
                        e.Actor.Log?.Write($"$Action.YouStepOverSeveral$ {count} {features.Key}.");
                    }
                }
                e.Actor.Physics.Position = e.NewPosition;
                return true;
            });
            // ActionSystem.ActorLostEffect:
            // - Show an animation and play a sound
            yield return actionSystem.ActorGainedEffect.SubscribeHandler(e =>
            {
                if (!Player.CanSee(e.Actor))
                    return;
                var flags = e.Effect.Name.GetFlags();
                renderSystem.CenterOn(Player);
                if (flags.IsBuff)
                {
                    if (Player.CanHear(e.Actor))
                    {
                        Resources.Sounds.Get(SoundName.Buff, e.Actor.Position() - Player.Position()).Play();
                    }
                    renderSystem.AnimateViewport(true, e.Actor, Animation.Buff(ColorName.LightCyan));
                }
                if (flags.IsDebuff)
                {
                    if (Player.CanHear(e.Actor))
                    {
                        Resources.Sounds.Get(SoundName.Debuff, e.Actor.Position() - Player.Position()).Play();
                    }
                    renderSystem.AnimateViewport(true, e.Actor, Animation.Debuff(ColorName.LightMagenta));
                }
                renderSystem.CenterOn(Player);
            });
            // ActionSystem.ActorLostEffect:
            yield return actionSystem.ActorLostEffect.SubscribeHandler(e =>
            {
            });
            // ActionSystem.ActorLeveledUp:
            // - Play jingle and animation
            yield return actionSystem.ActorLeveledUp.SubscribeHandler(e =>
            {
                if (e.Actor.IsPlayer())
                    Resources.Sounds.Get(SoundName.PlayerLevelUp).Play();
                else
                    Resources.Sounds.Get(SoundName.MonsterLevelUp, e.Actor.Position() - Player.Position()).Play();
                renderSystem.AnimateViewport(true, e.Actor, Animation.LevelUp(e.Actor));
            });
            // ActionSystem.ActorAttacked:
            // - Handle Ai aggro and grudges
            // - Show melee attack animation
            // - Identify wands and potions
            // - Occasionally play speech bubbles for attacker and victim
            yield return actionSystem.ActorAttacked.SubscribeResponse(e =>
            {
                foreach (var victim in e.Victims)
                {
                    e.Attacker.Log?.Write($"$Action.YouAttack$ {victim.Info.Name}.");
                    victim.Log?.Write($"{e.Attacker.Info.Name} $Action.AttacksYou$.");
                    var dir = (victim.Position() - e.Attacker.Position()).Clamp(-1, 1);
                    var speechChance = () => Chance.OneIn(15);
                    if (e.Type == AttackName.Melee)
                    {
                        if (Player.CanHear(e.Attacker) || Player.CanHear(victim))
                        {
                            Resources.Sounds.Get(SoundName.MeleeAttack, e.Attacker.Position() - Player.Position()).Play();
                        }
                        if (Player.CanSee(e.Attacker))
                        {
                            renderSystem.CenterOn(Player);
                            var anim = Animation.MeleeAttack(e.Attacker, dir)
                                .OnFirstFrame(() =>
                                {
                                    if (!e.Attacker.IsInvalid())
                                        e.Attacker.Render.Hidden = true;
                                    renderSystem.CenterOn(Player);
                                })
                                .OnLastFrame(() =>
                                {
                                    if (!e.Attacker.IsInvalid())
                                        e.Attacker.Render.Hidden = false;
                                    renderSystem.CenterOn(Player);
                                });
                            renderSystem.AnimateViewport(true, e.Attacker.Location(), anim);
                        }
                    }
                    if (speechChance() && Resources.GetSpeechBubble(e.Attacker, SpeechName.Attacking, out var speech))
                        renderSystem.AnimateViewport(false, e.Attacker, speech.Animation);
                    if (speechChance() && Resources.GetSpeechBubble(victim, SpeechName.Attacked, out speech))
                        renderSystem.AnimateViewport(false, victim, speech.Animation);
                    foreach (var weapon in e.Weapons)
                    {
                        if (e.Type == AttackName.Ranged && weapon.TryCast<Potion>(out var potion))
                        {
                            if (e.Attacker.Identify(potion, q => q.PotionProperties.QuaffEffect.Name == potion.PotionProperties.QuaffEffect.Name
                                                              && q.PotionProperties.ThrowEffect.Name == potion.PotionProperties.ThrowEffect.Name))
                            {
                                e.Attacker.Log?.Write($"$Action.YouIdentifyAPotion$ {potion.DisplayName}.");
                            }
                        }
                        else if (e.Type == AttackName.Magic && weapon.TryCast<Wand>(out var wand))
                        {

                            if (e.Attacker.Identify(wand, q => q.WandProperties.Effect.Name == wand.WandProperties.Effect.Name))
                            {
                                e.Attacker.Log?.Write($"$Action.YouIdentifyAWand$ {wand.DisplayName}.");
                            }
                        }
                    }

                }
                return true;
            });
            // ActionSystem.ActorHealed 
            // - Heal actor
            // - Show damage numbers
            yield return actionSystem.ActorHealed.SubscribeResponse(e =>
            {
                int oldHealth = e.Target.ActorProperties.Health;
                e.Target.ActorProperties.Health.V += e.Heal;
                var actualHeal = e.Target.ActorProperties.Health - oldHealth;
                if (Player.CanSee(e.Target))
                {
                    renderSystem.AnimateViewport(false, e.Target, Animation.DamageNumber(actualHeal, tint: ColorName.LightGreen));
                }
                return true;
            });
            // ActionSystem.ActorDamaged 
            // - Deal damage
            // - Handle aggro
            // - Show damage numbers
            yield return actionSystem.ActorDamaged.SubscribeResponse(e =>
            {
                if (e.Source.TryCast<Actor>(out var attacker))
                {
                    // force AI to recalculate 
                    if (e.Victim.Ai != null)
                        e.Victim.Ai.Objectives.Clear();
                    // make sure that people hold a grudge regardless of factions
                    factionSystem.SetUnilateralRelation(e.Victim, attacker, StandingName.Hated);
                }
                int oldHealth = e.Victim.ActorProperties.Health;
                e.Victim.ActorProperties.Health.V -= e.Damage;
                e.Victim.ActorProperties.LastTookDamageOnTurn = actionSystem.CurrentTurn;
                var actualDamage = oldHealth - e.Victim.ActorProperties.Health;
                if (Player.CanSee(e.Victim))
                {
                    var color = e.Victim.IsPlayer() ? ColorName.LightRed : ColorName.LightCyan;
                    var anim = e.IsCrit
                        ? Animation.DamageNumber_Crit(Math.Abs(actualDamage), tint: color)
                        : Animation.DamageNumber(Math.Abs(actualDamage), tint: color);
                    if (e.Victim.ActorProperties.Health.V <= 0)
                        renderSystem.AnimateViewport(false, e.Victim.Location(), anim);
                    else
                        renderSystem.AnimateViewport(false, e.Victim, anim);
                }
                return true;
            });
            // ActionSystem.ActorSpawned:
            // - Speech bubble (Spawned)
            yield return actionSystem.ActorSpawned.SubscribeResponse(e =>
            {
                if (Resources.GetSpeechBubble(e.Actor, SpeechName.Spawned, out var speech))
                {
                    renderSystem.AnimateViewport(false, e.Actor, speech.Animation);
                }
                return true;
            });
            // ActionSystem.EntityDespawned:
            // - Handle game over when the player dies
            // - Remove entity from floor and action systems and handle cleanup
            // - Generate a new RNG seed
            yield return actionSystem.EntityDespawned.SubscribeResponse(e =>
            {
                Entities.FlagEntityForRemoval(e.Entity.Id);
                if (e.Entity.TryCast<Actor>(out var actor))
                {
                    var wasPlayer = actor.IsPlayer();
                    actionSystem.StopTracking(actor.Id);
                    dungeonSystem.RemoveActor(actor);
                    actor.TryRefresh(0);
                    if (wasPlayer)
                    {
                        GenerateNewRngSeed();
                        TrySetState(SceneState.Main);
                    }
                }
                Entities.RemoveFlaggedItems(true);
                return true;
            });
            // ActionSystem.ActorDied:
            // - Play death animation
            // - Drop inventory contents
            // - Spawn corpses
            yield return actionSystem.ActorDied.SubscribeResponse(e =>
            {
                // Refresh entity in case a script raised this event
                if (!Entities.TryGetProxy<Actor>(e.Actor.Id, out var actor))
                    return true;

                e.Actor.Log?.Write($"$Action.YouDie$.");
                if (e.Actor.FloorId() == Player.FloorId())
                {
                    if (e.Actor.IsPlayer())
                    {
                        Resources.Sounds.Get(SoundName.PlayerDeath).Play();
                    }
                    else if (Player.CanHear(e.Actor))
                    {
                        Resources.Sounds.Get(SoundName.EnemyDeath, e.Actor.Position() - Player.Position()).Play();
                    }
                }
                e.Actor.Render.Hidden = true;

                Corpse corpse = null;
                var corpseDef = e.Actor.ActorProperties.Corpse;
                if (corpseDef.Type != CorpseName.None && corpseDef.Chance.Check(Rng.Random))
                {
                    corpse = Resources.Entities.Corpse(corpseDef.Type).Build();
                    actionSystem.CorpseCreated.HandleOrThrow(new(e.Actor, corpse));
                }

                if (Player.CanSee(e.Actor))
                {
                    if (corpse != null) corpse.Render.Hidden = true;
                    renderSystem.CenterOn(Player);
                    renderSystem.AnimateViewport(false, e.Actor.Location(), Animation.Death(e.Actor)
                        .OnLastFrame(() => { if (corpse != null) corpse.Render.Hidden = false; }));
                }

                if (e.Actor.Inventory != null)
                {
                    foreach (var item in e.Actor.Inventory.GetItems().ToList())
                    {
                        actionSystem.ItemDropped.HandleOrThrow(new(e.Actor, item));
                    }
                }
                return true;
            });
            // ActionSystem.ActorKilled:
            yield return actionSystem.ActorKilled.SubscribeResponse(e =>
            {
                e.Victim.Log?.Write($"{e.Killer.Info.Name} $Action.KillsYou$.");
                e.Killer.Log?.Write($"$Action.YouKill$ {e.Victim.Info.Name}.");
                return true;
            });
            // ActionSystem.ItemDropped:
            // - Drop item (remove from actor's inventory and add to floor)
            yield return actionSystem.ItemDropped.SubscribeResponse(e =>
            {
                if (e.Actor.Inventory.TryTake(e.Item))
                {
                    e.Item.Physics.Position = e.Actor.Position();
                    if (Systems.TryPlace(e.Actor.FloorId(), e.Item))
                    {
                        e.Actor.Log?.Write($"$Action.YouDrop$ {e.Item.DisplayName}.");
                    }
                    return true;
                }
                else
                {
                    e.Actor.Log?.Write($"$Action.UnableToDrop$ {e.Item.DisplayName}.");
                    return false;
                }
            });
            // ActionSystem.CorpseCreated:
            // - Create corpse item
            yield return actionSystem.CorpseCreated.SubscribeResponse(e =>
            {
                e.Corpse.Physics.Position = e.Actor.Position();
                if (Systems.TryPlace(e.Actor.FloorId(), e.Corpse))
                {
                    e.Actor.Log?.Write($"$Action.YouLeaveACorpse$.");
                    if (Player.CanSee(e.Corpse))
                        renderSystem.CenterOn(Player);
                }
                return true;
            });
            // ActionSystem.CorpseRaised:
            // - Spawn undead
            // - Play animation
            // - Destroy leftover corpse
            yield return actionSystem.CorpseRaised.SubscribeResponse(e =>
            {
                var faction = e.Source.TryCast<Actor>(out var necro)
                    ? necro.Faction.Name : FactionName.Monsters;
                var actorBuilder = e.Corpse.CorpseProperties.Type switch
                {
                    CorpseName.RatCorpse => Raise(Resources.Entities.NPC_RatZombie(), Resources.Entities.NPC_RatSkeleton(), e.Mode),
                    _ => throw new NotImplementedException()
                };
                var undead = actorBuilder
                    .WithFaction(faction)
                    .Build();
                undead.Physics.Position = e.Corpse.Position();
                if (!Systems.TrySpawn(e.Corpse.FloorId(), undead))
                {
                    necro?.Log?.Write($"$Action.NoRoomToRaiseUndead$.");
                    Entities.FlagEntityForRemoval(undead.Id);
                }
                if (Player.CanHear(undead))
                {
                    Resources.Sounds.Get(SoundName.Buff, undead.Position() - Player.Position()).Play();
                }
                if (Player.CanSee(undead))
                {
                    renderSystem.AnimateViewport(false, undead, Animation.Buff(ColorName.Magenta));
                }
                undead.TryJoinParty(necro);
                return actionSystem.CorpseDestroyed.Handle(new(e.Corpse));

                IEntityBuilder<Actor> Raise(IEntityBuilder<Actor> zombie, IEntityBuilder<Actor> skeleton, UndeadRaisingName mode)
                {
                    return mode switch
                    {
                        UndeadRaisingName.Zombie => zombie,
                        UndeadRaisingName.Skeleton => skeleton,
                        UndeadRaisingName.Random => Rng.Random.Choose(new[] { zombie, skeleton }),
                        _ => throw new NotImplementedException()
                    };
                }
            });
            // ActionSystem.CorpseDestroyed:
            // - Destroy corpse item
            yield return actionSystem.CorpseDestroyed.SubscribeResponse(e =>
            {
                dungeonSystem.RemoveItem(e.Corpse);
                Entities.FlagEntityForRemoval(e.Corpse.Id);
                return true;
            });
            // ActionSystem.ItemPickedUp:
            // - Store item in inventory or fail
            // - Play a sound if it's the player
            // - Show a sprite bubble on the actor that picked up the item
            yield return actionSystem.ItemPickedUp.SubscribeResponse(e =>
            {
                if (e.Actor.Inventory.TryPut(e.Item, out var fullyMerged))
                {
                    dungeonSystem.RemoveItem(e.Item);
                    if (fullyMerged)
                    {
                        Entities.FlagEntityForRemoval(e.Item.Id);
                    }
                    e.Actor.Log?.Write($"$Action.YouPickUpA$ {e.Item.DisplayName}.");
                    if (e.Actor.IsPlayer() && Player.CanHear(Player))
                    {
                        Resources.Sounds.Get(SoundName.ItemPickedUp).Play();
                    }
                    var spriteBubble = new Animation.SpriteBubble(TimeSpan.FromSeconds(1), e.Item.Render.Sprite, e.Item.Render.Color);
                    renderSystem.AnimateViewport(false, e.Actor, spriteBubble.Animation);
                    return true;
                }
                else
                {
                    e.Actor.Log?.Write($"$Action.YourInventoryIsTooFullFor$ {e.Item.DisplayName}.");
                    return false;
                }
            });
            // ActionSystem.ItemEquipped:
            // - Equip item or fail
            yield return actionSystem.ItemEquipped.SubscribeResponse(e =>
            {
                if (e.Actor.ActorEquipment.TryEquip(e.Item))
                {
                    e.Actor.Log?.Write($"$Action.YouEquip$ {e.Item.DisplayName}.");
                    return true;
                }
                else
                {
                    e.Actor.Log?.Write($"$Action.YouFailEquipping$ {e.Item.DisplayName}.");
                    return false;
                }
            });
            // ActionSystem.ItemUnequipped:
            // - Unequip item or fail
            yield return actionSystem.ItemUnequipped.SubscribeResponse(e =>
            {
                if (e.Actor.ActorEquipment.TryUnequip(e.Item))
                {
                    e.Actor.Log?.Write($"$Action.YouUnequip$ {e.Item.DisplayName}.");
                    return true;
                }
                else
                {
                    e.Actor.Log?.Write($"$Action.YouFailUnequipping$ {e.Item.DisplayName}.");
                    return false;
                }
            });
            // ActionSystem.ItemThrown:
            // - Spawn a 1-charge item where the consumable lands if it doesn't mulch
            // - Play an animation and a sound as the projectile flies
            yield return actionSystem.ItemThrown.SubscribeResponse(e =>
            {
                var proj = e.Projectile;
                e.Actor.Log?.Write($"$Action.YouThrow$ {proj.DisplayName}.");
                if (Player.CanHear(e.Actor))
                {
                    Resources.Sounds.Get(SoundName.RangedAttack, e.Actor.Position() - Player.Position()).Play();
                }
                if (Player.CanSee(e.Actor) || Player.CanSee(e.Victim))
                {
                    renderSystem.CenterOn(Player);
                    var anim = proj.ProjectileProperties.Trajectory switch
                    {
                        TrajectoryName.Arc => Animation.ArcingProjectile(e.Position - e.Actor.Position(), sprite: proj.Render.Sprite, tint: proj.Render.Color, directional: proj.ProjectileProperties.Directional, trailSprite: proj.ProjectileProperties.TrailSprite),
                        _ => Animation.StraightProjectile(e.Position - e.Actor.Position(), sprite: proj.Render.Sprite, tint: proj.Render.Color, directional: proj.ProjectileProperties.Directional, trailSprite: proj.ProjectileProperties.TrailSprite)
                    };
                    renderSystem.AnimateViewport(true, e.Actor.Location(), anim);
                    if (Player.CanHear(e.Actor) || Player.CanHear(e.Victim))
                    {
                        Resources.Sounds.Get(SoundName.MeleeAttack, e.Position - Player.Position()).Play();
                    }
                }
                var noMulch = Rng.Random.NextDouble() >= proj.ProjectileProperties.MulchChance;
                if (noMulch)
                {
                    var clone = (Projectile)proj.Clone();
                    if (proj.ProjectileProperties.ThrowsUseCharges)
                    {
                        clone.ConsumableProperties.RemainingUses = 1;
                    }
                    clone.Physics.Position = e.Position;
                    dungeonSystem.AddItem(e.Actor.FloorId(), clone);
                }
                else
                {
                    renderSystem.AnimateViewport(false, new Location(proj.FloorId(), e.Position), Animation.Explosion(tint: ColorName.Gray, scale: new(0.5f, 0.5f))); // mulch animation
                }
                if (!proj.ProjectileProperties.ThrowsUseCharges)
                {
                    // Despawn item
                    e.Actor.Inventory.TryTake(proj);
                    Entities.FlagEntityForRemoval(proj.Id);
                }
                return true;
            });
            // ActionSystem.WandZapped:
            // - Play an animation and a sound as the projectile flies
            yield return actionSystem.WandZapped.SubscribeResponse(e =>
            {
                e.Actor.Log?.Write($"$Action.YouZap$ {e.Wand.DisplayName}.");
                if (Player.CanHear(e.Actor))
                {
                    Resources.Sounds.Get(SoundName.MagicAttack, e.Actor.Position() - Player.Position()).Play();
                }
                if (Player.CanSee(e.Actor) || Player.CanSee(e.Victim))
                {
                    var anim = Animation.StraightProjectile(e.Position - e.Actor.Position(), sprite: e.Wand.Render.Sprite, tint: e.Wand.Render.Color, directional: true);
                    renderSystem.AnimateViewport(true, e.Actor.Location(), anim);
                    if (Player.CanHear(e.Actor) || Player.CanHear(e.Victim))
                    {
                        Resources.Sounds.Get(SoundName.MeleeAttack, e.Position - Player.Position()).Play();
                    }
                }
                if (e.Victim != null)
                {
                    if (e.Actor.Identify(e.Wand, q => q.WandProperties.Effect.Name == e.Wand.WandProperties.Effect.Name))
                    {
                        e.Actor.Log?.Write($"$Action.YouIdentify$ {e.Wand.DisplayName}.");
                    }
                }
                return true;
            });
            // ActionSystem.LauncherShot:
            // - Play an animation and a sound as the projectile flies
            yield return actionSystem.LauncherShot.SubscribeResponse(e =>
            {
                e.Actor.Log?.Write($"$Action.YouShoot$ {e.Launcher.DisplayName}.");
                if (Player.CanHear(e.Actor))
                {
                    Resources.Sounds.Get(SoundName.RangedAttack, e.Actor.Position() - Player.Position()).Play();
                }
                var proj = e.Launcher.LauncherProperties.Projectile;

                if (Player.CanSee(e.Actor) || Player.CanSee(e.Victim))
                {
                    var anim = proj.ProjectileProperties.Trajectory switch
                    {
                        TrajectoryName.Arc => Animation.ArcingProjectile(e.Position - e.Actor.Position(), sprite: proj.Render.Sprite, tint: proj.Render.Color, directional: proj.ProjectileProperties.Directional, trailSprite: proj.ProjectileProperties.TrailSprite),
                        _ => Animation.StraightProjectile(e.Position - e.Actor.Position(), sprite: proj.Render.Sprite, tint: proj.Render.Color, directional: proj.ProjectileProperties.Directional, trailSprite: proj.ProjectileProperties.TrailSprite),
                    };
                    renderSystem.AnimateViewport(true, e.Actor.Location(), anim);
                    if (Player.CanHear(e.Actor) || Player.CanHear(e.Victim))
                    {
                        Resources.Sounds.Get(SoundName.MeleeAttack, e.Position - Player.Position()).Play();
                    }
                }
                if (e.Victim != null)
                {
                    if (e.Actor.Identify(e.Launcher, q => true))
                    {
                        e.Actor.Log?.Write($"$Action.YouIdentify$ {e.Launcher.DisplayName}.");
                    }
                }
                return true;
            });
            // ActionSystem.ItemConsumed:
            // - Use up item and remove it from the game when completely consumed
            yield return actionSystem.ItemConsumed.SubscribeResponse(e =>
            {
                if (e.Actor.TryUseItem(e.Item, out var consumed))
                {
                    if (consumed)
                    {
                        Entities.FlagEntityForRemoval(e.Item.Id);
                    }
                    return true;
                }
                else
                {
                    e.Actor.Log?.Write($"$Action.YouFailUsing$ {e.Item.DisplayName}.");
                    return false;
                }
            });
            // ActionSystem.PotionQuaffed:
            // - Identify potions
            yield return actionSystem.PotionQuaffed.SubscribeResponse(e =>
            {
                e.Actor.Log?.Write($"$Action.YouQuaff$ {e.Potion.DisplayName}.");
                if (e.Actor.Identify(e.Potion, q => q.PotionProperties.QuaffEffect.Name == e.Potion.PotionProperties.QuaffEffect.Name
                                                 && q.PotionProperties.ThrowEffect.Name == e.Potion.PotionProperties.ThrowEffect.Name))
                {
                    e.Actor.Log?.Write($"$Action.YouIdentify$ {e.Potion.DisplayName}.");
                }
                return true;
            });
            // ActionSystem.ScrollRead:
            // - Identify scrolls
            yield return actionSystem.ScrollRead.SubscribeResponse(e =>
            {
                e.Actor.Log?.Write($"$Action.YouRead$ {e.Scroll.DisplayName}.");
                if (e.Actor.Identify(e.Scroll, q => q.ScrollProperties.Effect.Name == e.Scroll.ScrollProperties.Effect.Name))
                {
                    e.Actor.Log?.Write($"$Action.YouIdentify$ {e.Scroll.DisplayName}.");
                }
                return true;
            });
            // ActionSystem.SpellLearned:
            // - Add spell to actor's library
            // - Play sound and animation if player
            yield return actionSystem.SpellLearned.SubscribeResponse(e =>
            {
                if (!e.Actor.Spells.Learn(e.Spell))
                    return false;
                e.Actor.Log?.Write($"$Action.YouLearn$ {e.Spell.Info.Name}.");
                if (e.Actor.IsPlayer())
                {
                    // TODO: Play sound and animation if player
                }
                return true;
            });
            // ActionSystem.SpellForgotten:
            // - Remove spell from actor's library
            // - Play sound and animation if player
            yield return actionSystem.SpellForgotten.SubscribeResponse(e =>
            {
                if (!e.Actor.Spells.Forget(e.Spell))
                    return false;
                e.Actor.Log?.Write($"$Action.YouForget$ {e.Spell.Info.Name}.");
                if (e.Actor.IsPlayer())
                {
                    // TODO: Play sound and animation if player
                }
                return true;
            });
            // ActionSystem.SpellTargeted:
            // - Check effect preconditions
            yield return actionSystem.SpellTargeted.SubscribeResponse(e =>
            {
                foreach (var effect in e.Spell.Effects.Active)
                {
                    if (effect is CastEffect cast && !cast.ShouldApply(Systems, e.Spell, e.Actor))
                    {
                        return false;
                    }
                }
                return true;
            });
            // ActionSystem.SpellCast:
            yield return actionSystem.SpellCast.SubscribeResponse(e =>
            {
                return true;
            });
            // ActionSystem.ActorBumpedObstacle:
            // - Play a sound when it's the player
            yield return actionSystem.ActorBumpedObstacle.SubscribeHandler(e =>
            {
                if (e.Obstacle != null)
                {
                    //e.Actor.Log?.Write($"$Action.YouBumpInto$ {e.Obstacle.Info.Name}.");
                    if (e.Actor.IsPlayer())
                    {
                        Resources.Sounds.Get(SoundName.WallBump, e.Obstacle.Position() - Player.Position()).Play();
                    }
                }
                else
                {
                    //e.Actor.Log?.Write($"$Action.YouBumpIntoTheVoid$.");
                }
            });
            // ActionSystem.ActorSteppedOnTrap:
            // - Show an animation if the player can see this
            yield return actionSystem.ActorSteppedOnTrap.SubscribeHandler(e =>
            {
                e.Actor.Log?.Write($"$Action.YouTriggerATrap$.");
                if (Player.CanSee(e.Actor))
                {
                    var pos = e.Feature.Position();
                    renderSystem.CenterOn(Player);
                    Resources.Sounds.Get(SoundName.TrapSpotted, pos - Player.Position()).Play();
                    renderSystem.AnimateViewport(false, new Location(e.Actor.FloorId(), pos), Animation.ExpandingRing(5, tint: ColorName.LightBlue));
                }
            });
            // ActionSystem.ExplosionHappened:
            // - Show an animation and play sound
            // - Damage all affected entities
            yield return actionSystem.ExplosionHappened.SubscribeResponse(e =>
            {
                if (Player.IsAlive() && Player.FloorId() != e.FloorId)
                    return true;
                Resources.Sounds.Get(SoundName.Explosion, e.Center - Player.Position()).Play();
                if (Player.CanSee(e.FloorId, e.Center))
                {
                    renderSystem.AnimateViewport(false, new Location(e.FloorId, e.Center), e.Points.Select(p => Animation.Explosion(offset: (p - e.Center).ToVec())).ToArray());
                }
                foreach (var p in e.Points)
                {
                    foreach (var a in dungeonSystem.GetActorsAt(e.FloorId, p))
                    {
                        var damage = (int)(e.BaseDamage / (a.SquaredDistanceFrom(e.Center) + 1));
                        actionSystem.ActorDamaged.HandleOrThrow(new(e.Cause, a, [e.Source], damage, false));
                    }
                }
                return true;
            });
            // ActionSystem.CriticalHitHappened:
            // - Log the event
            // - Show an animation and play sound
            yield return actionSystem.CriticalHitHappened.SubscribeResponse(e =>
            {
                e.Attacker.Log.Write($"$Action.YouCritFor$ {e.Damage} $Action.Damage$.");
                if (Player.IsAlive() && Player.FloorId() != e.Attacker.FloorId())
                    return true;
                var pos = e.Attacker.Position();
                Resources.Sounds.Get(SoundName.Crit, pos - Player.Position()).Play();
                if (Player.CanSee(pos))
                {

                }
                return true;
            });
            // ActionSystem.FeatureInteractedWith:
            // - Open/close doors
            // - Handle shrine interactions
            // - Handle chest interactions
            // - Handle stair and portal interactions
            // - Empty and fill action queue on player floor change
            yield return actionSystem.FeatureInteractedWith.SubscribeResponse(e =>
            {
                if (e.Feature.TryToggleDoor())
                {
                    if (e.Feature.IsDoorClosed())
                    {
                        e.Actor.Log?.Write($"$Action.YouCloseThe$ {e.Feature.Info.Name}.");
                    }
                    else
                    {
                        e.Actor.Log?.Write($"$Action.YouOpenThe$ {e.Feature.Info.Name}.");
                    }
                    return true;
                }
                if (e.Feature.FeatureProperties.Name == FeatureName.Shrine)
                {
                    e.Actor.Log?.Write($"$Action.YouKneelAt$ {e.Feature.Info.Name}.");
                    return true;
                }
                if (e.Feature.FeatureProperties.Name == FeatureName.Chest)
                {
                    return HandleChest();
                }
                if (e.Feature.TryCast<Portal>(out var portal))
                {
                    if (e.Feature.FeatureProperties.Name == FeatureName.Upstairs)
                    {
                        return HandleStairs(portal.PortalProperties.Connection.To, portal.PortalProperties.Connection.From);
                    }
                    if (e.Feature.FeatureProperties.Name == FeatureName.Downstairs)
                    {
                        return HandleStairs(portal.PortalProperties.Connection.From, portal.PortalProperties.Connection.To);
                    }
                }
                return false;

                bool HandleChest()
                {
                    if (!e.Actor.IsPlayer())
                    {
                        return false; // Sorry monsters
                    }
                    var chestRng = Rng.SeededRandom(e.Feature.Id);
                    if (Chance.Check(chestRng, 1, 10))
                    {
                        // Spawn a mimic, log a message and play a sound
                        var enemy = Resources.Entities.NPC_Mimic()
                            .WithPosition(e.Feature.Position())
                            .Build();
                        var items = e.Feature.Inventory.GetItems().ToList();
                        RemoveFeature();
                        if (Systems.TrySpawn(e.Feature.FloorId(), enemy))
                        {
                            foreach (var item in items)
                            {
                                enemy.Inventory.TryPut(item, out _);
                            }
                            e.Actor.Log?.Write($"$Action.TheChestWasAMimic$");
                        }
                        return true;
                    }
                    // Show inventory modal of chest contents
                    var canPutItemsInInventory = !e.Actor.Inventory?.Full ?? false;
                    var chest = UI.Chest(e.Feature, canPutItemsInInventory, e.Feature.Info.Name);
                    chest.ActionPerformed += (item, action) =>
                    {
                        switch (action)
                        {
                            case ChestActionName.Drop:
                                item.Physics.Position = e.Feature.Position();
                                Systems.TryPlace(e.Feature.FloorId(), item);
                                e.Feature.Inventory.TryTake(item);
                                break;
                            case ChestActionName.Take:
                                e.Actor.Inventory.TryPut(item, out var fullyMerged);
                                e.Feature.Inventory.TryTake(item);
                                break;
                        }
                        if (e.Feature.Inventory.Empty)
                        {
                            RemoveFeature();
                        }
                    };
                    return true;

                    void RemoveFeature()
                    {
                        dungeonSystem.RemoveFeature(e.Feature);
                        Entities.FlagEntityForRemoval(e.Feature.Id);
                    }
                }

                bool HandleStairs(FloorId current, FloorId next)
                {
                    var currentFloor = dungeonSystem.GetFloor(current);
                    if (!dungeonSystem.TryGetFloor(next, out var nextFloor))
                    {
                        e.Actor.Log?.Write($"$Error.NoFloorWithId$ {next}.");
                        return false;
                    }
                    var stairs = dungeonSystem.GetAllFeatures(next)
                        .TrySelect(f => (f.TryCast<Portal>(out var portal), portal))
                        .OrderBy(x => x.DistanceFrom(e.Feature))
                        .First(f => f.PortalProperties.Connects(current, next));
                    actionSystem.StopTracking(e.Actor.Id);
                    if (e.Actor.IsPlayer())
                    {
                        foreach (var actor in dungeonSystem.GetAllActors(current))
                        {
                            actionSystem.StopTracking(actor.Id);
                        }
                    }
                    currentFloor.RemoveActor(e.Actor);
                    e.Actor.Physics.Position = stairs.Position();
                    e.Actor.Physics.FloorId = next;
                    nextFloor.AddActor(e.Actor);
                    e.Actor.Log?.Write($"$Action.YouTakeTheStairsTo$ {next}.");
                    if (e.Actor.IsPlayer())
                    {
                        foreach (var actor in dungeonSystem.GetAllActors(next))
                        {
                            actionSystem.Track(actor.Id);
                        }
                        // Abruptly stop the current turn so that the actor queue is flushed completely
                        actionSystem.AbortCurrentTurn();
                        dungeonSystem.RecalculateFov(Player);
                        renderSystem.CenterOn(Player);
                    }
                    actionSystem.StopTracking(e.Actor.Id);
                    return true;
                }
            });
        }

        public void GenerateNewRngSeed()
        {
            var newRngSeed = (int)DateTime.Now.ToBinary();
            Store.SetValue(Data.Global.RngSeed, newRngSeed);
            Rng.SetGlobalSeed(newRngSeed);
        }

        public override void Update(TimeSpan t, TimeSpan dt)
        {
            if (_newFrame)
            {
                Systems.Get<ActionSystem>().ElapseTurn(Player.Id);
                _newFrame = false;
            }
            Systems.Get<RenderSystem>().Update(t, dt);
            Systems.Get<InputSystem>().Update(t, dt);
            Systems.Get<MusicSystem>().Update(t, dt);
            if (UI.Input.IsKeyboardFocusAvailable && UI.Input.IsKeyPressed(VirtualKeys.R))
            {
                if (UI.Input.IsKeyDown(VirtualKeys.Shift))
                {
                    GenerateNewRngSeed();
                }
                else
                {
                    Rng.SetGlobalSeed(Store.Get(Data.Global.RngSeed));
                }
                TrySetState(SceneState.Main);
            }
        }

        public override void DrawBackground(RenderTarget target, RenderStates states)
        {
            UI.Window.Clear();
            Systems.Get<RenderSystem>().Draw(target, states);
        }

        public override void DrawForeground(RenderTarget target, RenderStates states)
        {
            _newFrame = true;
        }

        protected override bool CanChangeState(SceneState newState) => true;
        protected override void OnStateChanged(SceneState oldState)
        {
            base.OnStateChanged(oldState);
            foreach (var win in UI.GetOpenWindows().ToArray())
            {
                win.Close(default);
            }
            if (State == SceneState.Main)
            {
                Systems.Get<ActionSystem>().Reset();
                Systems.Get<RenderSystem>().Reset();
                Systems.Get<RenderSystem>().CenterOn(Player);
            }
        }
    }
}
