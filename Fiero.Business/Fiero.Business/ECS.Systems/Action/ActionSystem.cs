using Unconcern.Common;

namespace Fiero.Business
{

    public sealed partial class ActionSystem : EcsSystem
    {
        private const int TURN_ACTOR_ID = -1;

        private readonly List<ActorTime> _actorQueue = new();
        public int CurrentTurn { get; private set; }

        private readonly GameEntities _entities;
        private readonly DungeonSystem _floorSystem;
        private readonly FactionSystem _factionSystem;

        public int CurrentGeneration { get; private set; }

        public readonly SystemRequest<ActionSystem, TurnEvent, EventResult> GameStarted;
        public readonly SystemRequest<ActionSystem, TurnEvent, EventResult> TurnStarted;
        public readonly SystemRequest<ActionSystem, TurnEvent, EventResult> TurnEnded;
        public readonly SystemRequest<ActionSystem, ActorTurnEvent, EventResult> ActorTurnStarted;
        public readonly SystemRequest<ActionSystem, ActorTurnEvent, EventResult> ActorTurnEnded;
        public readonly SystemRequest<ActionSystem, PlayerTurnEvent, EventResult> PlayerTurnStarted;
        public readonly SystemRequest<ActionSystem, PlayerTurnEvent, EventResult> PlayerTurnEnded;
        public readonly SystemRequest<ActionSystem, ActorMovedEvent, EventResult> ActorMoved;
        public readonly SystemRequest<ActionSystem, ActorMovedEvent, EventResult> ActorTeleporting;
        public readonly SystemRequest<ActionSystem, ActorUsedMagicMappingEvent, EventResult> ActorUsedMagicMapping;
        public readonly SystemRequest<ActionSystem, ActorTurnEvent, EventResult> ActorWaited;
        public readonly SystemRequest<ActionSystem, ActorSpawnedEvent, EventResult> ActorSpawned;
        public readonly SystemRequest<ActionSystem, EntityDespawnedEvent, EventResult> EntityDespawned;
        public readonly SystemRequest<ActionSystem, ActorDiedEvent, EventResult> ActorDied;
        public readonly SystemRequest<ActionSystem, ActorKilledEvent, EventResult> ActorKilled;
        public readonly SystemRequest<ActionSystem, ActorGainedExperienceEvent, EventResult> ActorGainedExperience;
        public readonly SystemRequest<ActionSystem, ActorLeveledUpEvent, EventResult> ActorLeveledUp;
        public readonly SystemRequest<ActionSystem, ActorAttackedEvent, EventResult> ActorAttacked;
        public readonly SystemRequest<ActionSystem, ActorDamagedEvent, EventResult> ActorDamaged;
        public readonly SystemRequest<ActionSystem, ActorHealedEvent, EventResult> ActorHealed;
        public readonly SystemRequest<ActionSystem, SpellLearnedEvent, EventResult> SpellLearned;
        public readonly SystemRequest<ActionSystem, SpellForgottenEvent, EventResult> SpellForgotten;
        public readonly SystemRequest<ActionSystem, SpellCastEvent, EventResult> SpellCast;
        public readonly SystemRequest<ActionSystem, SpellTargetedEvent, EventResult> SpellTargeted;
        public readonly SystemRequest<ActionSystem, ItemPickedUpEvent, EventResult> ItemPickedUp;
        public readonly SystemRequest<ActionSystem, ItemDroppedEvent, EventResult> ItemDropped;
        public readonly SystemRequest<ActionSystem, CorpseCreatedEvent, EventResult> CorpseCreated;
        public readonly SystemRequest<ActionSystem, CorpseRaisedEvent, EventResult> CorpseRaised;
        public readonly SystemRequest<ActionSystem, CorpseDestroyedEvent, EventResult> CorpseDestroyed;
        public readonly SystemRequest<ActionSystem, ItemThrownEvent, EventResult> ItemThrown;
        public readonly SystemRequest<ActionSystem, WandZappedEvent, EventResult> WandZapped;
        public readonly SystemRequest<ActionSystem, LauncherShotEvent, EventResult> LauncherShot;
        public readonly SystemRequest<ActionSystem, ItemEquippedEvent, EventResult> ItemEquipped;
        public readonly SystemRequest<ActionSystem, ItemUnequippedEvent, EventResult> ItemUnequipped;
        public readonly SystemRequest<ActionSystem, ItemConsumedEvent, EventResult> ItemConsumed;
        public readonly SystemRequest<ActionSystem, ScrollReadEvent, EventResult> ScrollRead;
        public readonly SystemRequest<ActionSystem, PotionQuaffedEvent, EventResult> PotionQuaffed;
        public readonly SystemRequest<ActionSystem, FeatureInteractedWithEvent, EventResult> FeatureInteractedWith;
        public readonly SystemRequest<ActionSystem, ConversationInitiatedEvent, EventResult> ConversationInitiated;
        public readonly SystemRequest<ActionSystem, ExplosionHappenedEvent, EventResult> ExplosionHappened;
        public readonly SystemRequest<ActionSystem, CriticalHitHappenedEvent, EventResult> CriticalHitHappened;

        public readonly SystemEvent<ActionSystem, FeatureInteractedWithEvent> ActorSteppedOnTrap;
        public readonly SystemEvent<ActionSystem, ActorBumpedObstacleEvent> ActorBumpedObstacle;
        public readonly SystemEvent<ActionSystem, ActorGainedEffectEvent> ActorGainedEffect;
        public readonly SystemEvent<ActionSystem, ActorLostEffectEvent> ActorLostEffect;

        public readonly SystemRequest<ActionSystem, ActorIntentEvent, ReplaceIntentEventResult> ActorIntentSelected;
        public readonly SystemEvent<ActionSystem, ActorIntentEvent> ActorIntentEvaluated;
        public readonly SystemEvent<ActionSystem, ActorIntentEvent> ActorIntentFailed;

        public int CurrentActorId => _actorQueue[0].ActorId;
        public IEnumerable<int> ActorIds => _actorQueue.Select(x => x.ActorId);

        private bool _invalidate;

        public void AbortCurrentTurn() => _invalidate = true;

        public ActionSystem(
            EventBus bus,
            GameEntities entities,
            DungeonSystem floorSystem,
            FactionSystem factionSystem
        ) : base(bus)
        {
            _entities = entities;
            _floorSystem = floorSystem;
            _factionSystem = factionSystem;
            GameStarted = new(this, nameof(GameStarted));
            TurnStarted = new(this, nameof(TurnStarted));
            TurnEnded = new(this, nameof(TurnEnded));
            ActorTurnStarted = new(this, nameof(ActorTurnStarted));
            ActorTurnEnded = new(this, nameof(ActorTurnEnded));
            PlayerTurnStarted = new(this, nameof(PlayerTurnStarted));
            PlayerTurnEnded = new(this, nameof(PlayerTurnEnded));
            ActorMoved = new(this, nameof(ActorMoved));
            ActorTeleporting = new(this, nameof(ActorTeleporting));
            ActorUsedMagicMapping = new(this, nameof(ActorUsedMagicMapping));
            ActorSpawned = new(this, nameof(ActorSpawned));
            EntityDespawned = new(this, nameof(EntityDespawned));
            ActorWaited = new(this, nameof(ActorWaited));
            ActorDied = new(this, nameof(ActorDied));
            ActorKilled = new(this, nameof(ActorKilled));
            ActorGainedExperience = new(this, nameof(ActorGainedExperience));
            ActorLeveledUp = new(this, nameof(ActorLeveledUp));
            ActorAttacked = new(this, nameof(ActorAttacked));
            ActorDamaged = new(this, nameof(ActorDamaged));
            ActorHealed = new(this, nameof(ActorHealed));
            SpellLearned = new(this, nameof(SpellLearned));
            SpellForgotten = new(this, nameof(SpellForgotten));
            SpellCast = new(this, nameof(SpellCast));
            SpellTargeted = new(this, nameof(SpellTargeted));
            ItemPickedUp = new(this, nameof(ItemPickedUp));
            ItemDropped = new(this, nameof(ItemDropped));
            CorpseCreated = new(this, nameof(CorpseCreated));
            CorpseRaised = new(this, nameof(CorpseRaised));
            CorpseDestroyed = new(this, nameof(CorpseDestroyed));
            ItemThrown = new(this, nameof(ItemThrown));
            WandZapped = new(this, nameof(WandZapped));
            ScrollRead = new(this, nameof(ScrollRead));
            PotionQuaffed = new(this, nameof(PotionQuaffed));
            ItemEquipped = new(this, nameof(ItemEquipped));
            ItemUnequipped = new(this, nameof(ItemUnequipped));
            ItemConsumed = new(this, nameof(ItemConsumed));
            FeatureInteractedWith = new(this, nameof(FeatureInteractedWith));
            ExplosionHappened = new(this, nameof(ExplosionHappened));
            CriticalHitHappened = new(this, nameof(CriticalHitHappened));
            ActorSteppedOnTrap = new(this, nameof(ActorSteppedOnTrap));
            ActorBumpedObstacle = new(this, nameof(ActorBumpedObstacle));
            ActorGainedEffect = new(this, nameof(ActorGainedEffect));
            ActorLostEffect = new(this, nameof(ActorLostEffect));
            ActorIntentSelected = new(this, nameof(ActorIntentSelected));
            ActorIntentEvaluated = new(this, nameof(ActorIntentEvaluated));
            ActorIntentFailed = new(this, nameof(ActorIntentFailed));
            ConversationInitiated = new(this, nameof(ConversationInitiated));
            LauncherShot = new(this, nameof(LauncherShot));

            ActorAttacked.AllResponsesReceived += (_, e, r) =>
            {
                if (r.All(x => x))
                {
                    foreach (var victim in e.Victims)
                        ActorDamaged.HandleOrThrow(new(e.Attacker, victim, e.Weapons, e.Damage, e.IsCrit));
                }
            };
            ActorDamaged.AllResponsesReceived += (_, e, r) =>
            {
                if (r.All(x => x))
                {
                    if (e.Victim.IsAlive() && e.Victim.ActorProperties.Health <= 0)
                    {
                        if (e.Source.TryCast<Actor>(out var killer))
                        {
                            ActorKilled.HandleOrThrow(new(killer, e.Victim));
                        }
                        else
                        {
                            ActorDied.HandleOrThrow(new(e.Victim));
                        }
                    }
                }
            };
            ActorGainedExperience.AllResponsesReceived += (_, e, r) =>
            {
                if (r.All(x => x))
                {
                    if (e.Actor.IsAlive() && e.Actor.ActorProperties.XP >= e.Actor.ActorProperties.MaxXP)
                    {
                        e.Actor.ActorProperties.LVL += 1;
                        e.Actor.ActorProperties.XP = 0;
                        e.Actor.ActorProperties.MaxXP = XpToNextLevel(e.Actor.ActorProperties.LVL, e.Actor.ActorProperties.BaseXP, e.Actor.ActorProperties.XPExponent);
                        ActorLeveledUp.HandleOrThrow(new(e.Actor, e.Actor.ActorProperties.Level - 1, e.Actor.ActorProperties.Level));
                    }
                }
            };
            ActorLeveledUp.AllResponsesReceived += (_, e, r) =>
            {
                if (r.All(x => x))
                {
                    var numLevels = e.NewLevel - e.OldLevel;
                    if (e.Actor.IsAlive())
                    {
                        // TODO: Level up popup, increase stats, etc.
                        var hpIncrease = e.Actor.ActorProperties.HPGrowth.Roll().Sum();
                        var mpIncrease = e.Actor.ActorProperties.MPGrowth.Roll().Sum();
                        e.Actor.ActorProperties.MaxHP += hpIncrease;
                        e.Actor.ActorProperties.HP += hpIncrease;
                        e.Actor.ActorProperties.MaxMP += mpIncrease;
                        e.Actor.ActorProperties.MP += mpIncrease;
                    }
                }
            };
            ActorKilled.AllResponsesReceived += (_, e, r) =>
            {
                if (r.All(x => x))
                {
                    ActorDied.HandleOrThrow(new(e.Victim));
                }
            };
            ActorDied.AllResponsesReceived += (_, e, r) =>
            {
                if (r.All(x => x))
                {
                    EntityDespawned.HandleOrThrow(new(e.Actor));
                }
            };
            ResetImpl();
        }

        public int XpToNextLevel(int level, int baseXp, float exponent)
        {
            return (int)Math.Floor(baseXp * Math.Pow(level, exponent));
        }

        private int? HandleAction(ActorTime t, ref IAction action)
        {
            var ret = default(bool?);
            var cost = action.Cost;
            if (t.Actor.IsInvalid())
                return cost;
            if (t.ActorId == TURN_ACTOR_ID)
                return cost;

            switch (action.Name)
            {
                case ActionName.Wait:
                    ret = HandleWait(t, ref action, ref cost);
                    break;
                case ActionName.Move:
                    ret = HandleMove(t, ref action, ref cost);
                    // Randomize movement cost slightly
                    if (cost != null)
                        cost += (int)(Rng.Random.Between(-1, 1) * cost * 0.1);
                    break;
                case ActionName.MeleeAttack:
                    ret = HandleMeleeAttack(t, ref action, ref cost);
                    break;
                case ActionName.Throw:
                    ret = HandleThrowItem(t, ref action, ref cost);
                    break;
                case ActionName.Cast:
                    ret = HandleCastSpell(t, ref action, ref cost);
                    break;
                case ActionName.Interact:
                    ret = HandleInteract(t, ref action, ref cost);
                    break;
                case ActionName.Zap:
                    ret = HandleZapWand(t, ref action, ref cost);
                    break;
                case ActionName.Shoot:
                    ret = HandleShootLauncher(t, ref action, ref cost);
                    break;
                case ActionName.Read:
                    ret = HandleReadScroll(t, ref action, ref cost);
                    break;
                case ActionName.Quaff:
                    ret = HandleQuaffPotion(t, ref action, ref cost);
                    break;
                case ActionName.Organize:
                    ret = HandleOrganize(t, ref action, ref cost);
                    break;
                case ActionName.Macro:
                    ret = HandleMacro(t, ref action, ref cost);
                    break;
                case ActionName.Fail:
                    ret = false;
                    break;
            }
            if (!t.Actor.IsAlive())
            {
                action = new FailAction();
                return cost;
            }
            t.Actor.Action.LastAction = action;
            if (ret == true)
            {
                _ = ActorIntentEvaluated.Raise(new(t.Actor, action, CurrentTurn, t.Time));
            }
            else if (ret == false)
            {
                action = new FailAction();
                _ = ActorIntentFailed.Raise(new(t.Actor, action, CurrentTurn, t.Time));
            }
            return cost;
        }

        private void ResetImpl()
        {
            AbortCurrentTurn();
            _actorQueue.Clear();
            _actorQueue.Add(new ActorTime(TURN_ACTOR_ID, null, () => new WaitAction(), 0));
            CurrentTurn = 0;
        }

        public void Reset()
        {
            CurrentGeneration++;
            ResetImpl();
            _ = GameStarted.Raise(new());
        }

        public void Spawn(Actor a)
        {
            _ = ActorSpawned.Raise(new(a));
        }

        public void Despawn(Entity e)
        {
            _ = EntityDespawned.Raise(new(e));
        }

        private bool MayTarget(Actor attacker, Actor victim)
        {
            return victim.IsAlive() && (attacker.IsAffectedBy(EffectName.Confusion)
                || _factionSystem.GetRelations(attacker, victim).Left.IsHostile());
        }

        private bool TryFindVictim(Coord p, Actor attacker, out Actor victim)
        {
            victim = default;
            var actorsHere = _floorSystem.GetActorsAt(attacker.FloorId(), p);
            if (!actorsHere.Any(a => MayTarget(attacker, a)))
            {
                return false;
            }
            victim = actorsHere
                .Single(x => x.ActorProperties.Type != ActorName.None);
            return true;
        }

        private bool HandleAttack(AttackName type, Actor attacker, Actor[] victims, ref int? cost, Entity[] weapons, out int damage, out int swingDelay, out bool crit)
        {
            if (TryAttack(out damage, out swingDelay, out crit, type, attacker, victims, weapons))
            {
                cost += swingDelay;
                return true;
            }
            return false;
        }

        public bool TryAttack(out int damage, out int swingDelay, out bool crit, AttackName type, Actor attacker, Actor[] victims, Entity[] attackWith)
        {
            damage = 0; swingDelay = 0; crit = false;
            if (attackWith != null && attackWith.Length > 0)
            {
                foreach (var item in attackWith)
                {
                    if (type == AttackName.Melee && item.TryCast<Weapon>(out var w))
                    {
                        swingDelay += w.WeaponProperties.SwingDelay;
                        damage += w.WeaponProperties.BaseDamage.Roll().Sum();
                        if (w.WeaponProperties.CritChance.Check())
                        {
                            var critDamage = damage * 3;
                            crit = CriticalHitHappened.Handle(new(attacker, victims, w, critDamage));
                            if (crit)
                                damage = critDamage;
                        }
                    }
                    else if (type == AttackName.Ranged && item.TryCast<Projectile>(out var t))
                    {
                        damage += t.ProjectileProperties.BaseDamage;
                    }
                    else if (type == AttackName.Magic && item.TryCast<Spell>(out var s))
                    {
                        swingDelay = s.SpellProperties.CastDelay;
                        damage += s.SpellProperties.BaseDamage;
                    }
                    else if (type == AttackName.Magic && item.TryCast<Wand>(out _))
                    {
                        damage = 0;
                    }
                }
            }
            else damage = 1;
            return ActorAttacked.Handle(new(type, attacker, victims, attackWith, damage, swingDelay, crit));
        }

        public void Track(int actorId)
        {
            if (_actorQueue.Any(x => x.ActorId == actorId))
            {
                return;
            }
            // Actors have their energy randomized when spawning, to distribute them better across turns
            var time = _actorQueue.Single(x => x.ActorId == TURN_ACTOR_ID).Time;
            var proxy = _entities.GetProxy<Actor>(actorId);
            var currentTurn = CurrentTurn;
            _actorQueue.Add(new ActorTime(actorId, proxy, () =>
            {
                if (proxy.IsInvalid())
                    return new WaitAction();
                return proxy.Action.ActionProvider.GetIntent(proxy);
            }, time + Rng.Random.Next(0, 100)));
        }

        public void StopTracking(int actorId)
        {
            _actorQueue.RemoveAll(x => x.ActorId == actorId);
        }

        public int? ElapseTick()
        {
            var next = Dequeue();
            OnTurnStarted(next.ActorId);
            next = next.WithLastActedTime(next.Time);
            var intent = next.GetIntent();
            if (intent.Name != ActionName.None)
            {
                // Some effects might want to hook into this request to change the intent of an actor right before it's evaluated
                var altIntents = ActorIntentSelected.Request(new(next.Actor, intent, CurrentTurn, next.Time))
                    .ToBlockingEnumerable()
                    .Where(i => i.Result)
                    .OrderByDescending(i => i.Priority);
                if (altIntents.FirstOrDefault() is { } altIntent)
                {
                    intent = altIntent.NewIntent;
                }
            }
            if (HandleAction(next, ref intent) is { } cost)
            {
                if (_invalidate)
                {
                    _invalidate = false;
                    if (!_actorQueue.Any(a => a.ActorId == next.ActorId))
                    {
                        _actorQueue.Add(next);
                    }
                    return cost;
                }
                OnTurnEnded(next.ActorId);
                if (next.Actor is { Action: { } a })
                    a.TurnsSurvived++;
            }
            else
            {
                _actorQueue.Insert(0, next);
                return null;
            }
            next = next.WithTime(next.Time + cost);
            if (next.ActorId == TURN_ACTOR_ID || next.Actor.IsAlive())
            {
                var index = _actorQueue.FindIndex(t => t.Time > next.Time);
                if (index < 0)
                {
                    _actorQueue.Add(next);
                }
                else
                {
                    _actorQueue.Insert(index, next);
                }
            }
            return cost;

            ActorTime Dequeue()
            {
                var next = _actorQueue[0];
                _actorQueue.RemoveAt(0);
                return next;
            }

            void OnTurnStarted(int actorId)
            {
                if (actorId == TURN_ACTOR_ID)
                {
                    _ = TurnStarted.Raise(new(++CurrentTurn));
                }
                else if (next.LastActedTime < next.Time)
                {
                    _ = ActorTurnStarted.Raise(new(next.Actor, CurrentTurn, next.Time));
                    if (next.Actor.IsPlayer())
                        _ = PlayerTurnStarted.Raise(new(next.Actor.Id, CurrentTurn, next.Time));
                }
            }

            void OnTurnEnded(int actorId)
            {
                if (actorId == TURN_ACTOR_ID)
                {
                    _ = TurnEnded.Raise(new(CurrentTurn));
                }
                else
                {
                    _ = ActorTurnEnded.Raise(new(next.Actor, CurrentTurn, next.Time));
                    if (next.Actor.IsPlayer())
                        _ = PlayerTurnEnded.Raise(new(next.Actor.Id, CurrentTurn, next.Time));
                }
            }
        }

        /// <summary>
        /// Processes ticks until it's the player's turn, then yields control.
        /// </summary>
        /// <param name="playerId"></param>
        public void ElapseTurn(int playerId)
        {
            do
            {
                if (ElapseTick() is { })
                {
                    // If the player dies and is removed from the entities, it will pass by RemoveFlagged.
                    // This way we can avoid constantly looking for the player id in a collection.
                    foreach (var e in _entities.RemoveFlaggedItems(true))
                    {
                        if (e == playerId)
                            return;
                    }
                }
            }
            while (CurrentActorId != playerId);
        }
    }
}
