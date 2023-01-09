using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
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

        public readonly SystemRequest<ActionSystem, TurnEvent, EventResult> GameStarted;
        public readonly SystemRequest<ActionSystem, TurnEvent, EventResult> TurnStarted;
        public readonly SystemRequest<ActionSystem, TurnEvent, EventResult> TurnEnded;
        public readonly SystemRequest<ActionSystem, ActorTurnEvent, EventResult> ActorTurnStarted;
        public readonly SystemRequest<ActionSystem, ActorTurnEvent, EventResult> ActorTurnEnded;
        public readonly SystemRequest<ActionSystem, ActorMovedEvent, EventResult> ActorMoved;
        public readonly SystemRequest<ActionSystem, ActorMovedEvent, EventResult> ActorTeleporting;
        public readonly SystemRequest<ActionSystem, ActorTurnEvent, EventResult> ActorWaited;
        public readonly SystemRequest<ActionSystem, ActorSpawnedEvent, EventResult> ActorSpawned;
        public readonly SystemRequest<ActionSystem, ActorDespawnedEvent, EventResult> ActorDespawned;
        public readonly SystemRequest<ActionSystem, ActorDiedEvent, EventResult> ActorDied;
        public readonly SystemRequest<ActionSystem, ActorKilledEvent, EventResult> ActorKilled;
        public readonly SystemRequest<ActionSystem, ActorAttackedEvent, EventResult> ActorAttacked;
        public readonly SystemRequest<ActionSystem, ActorDamagedEvent, EventResult> ActorDamaged;
        public readonly SystemRequest<ActionSystem, ActorHealedEvent, EventResult> ActorHealed;
        public readonly SystemRequest<ActionSystem, SpellLearnedEvent, EventResult> SpellLearned;
        public readonly SystemRequest<ActionSystem, SpellForgottenEvent, EventResult> SpellForgotten;
        public readonly SystemRequest<ActionSystem, SpellCastEvent, EventResult> SpellCast;
        public readonly SystemRequest<ActionSystem, SpellTargetedEvent, EventResult> SpellTargeted;
        public readonly SystemRequest<ActionSystem, ItemPickedUpEvent, EventResult> ItemPickedUp;
        public readonly SystemRequest<ActionSystem, ItemDroppedEvent, EventResult> ItemDropped;
        public readonly SystemRequest<ActionSystem, ItemThrownEvent, EventResult> ItemThrown;
        public readonly SystemRequest<ActionSystem, WandZappedEvent, EventResult> WandZapped;
        public readonly SystemRequest<ActionSystem, ItemEquippedEvent, EventResult> ItemEquipped;
        public readonly SystemRequest<ActionSystem, ItemUnequippedEvent, EventResult> ItemUnequipped;
        public readonly SystemRequest<ActionSystem, ItemConsumedEvent, EventResult> ItemConsumed;
        public readonly SystemRequest<ActionSystem, ScrollReadEvent, EventResult> ScrollRead;
        public readonly SystemRequest<ActionSystem, PotionQuaffedEvent, EventResult> PotionQuaffed;
        public readonly SystemRequest<ActionSystem, FeatureInteractedWithEvent, EventResult> FeatureInteractedWith;
        public readonly SystemRequest<ActionSystem, ExplosionHappenedEvent, EventResult> ExplosionHappened;

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
        ) : base(bus) {
            _entities = entities;
            _floorSystem = floorSystem;
            _factionSystem = factionSystem;
            GameStarted = new(this, nameof(GameStarted));
            TurnStarted = new(this, nameof(TurnStarted));
            TurnEnded = new(this, nameof(TurnEnded));
            ActorTurnStarted = new(this, nameof(ActorTurnStarted));
            ActorTurnEnded = new(this, nameof(ActorTurnEnded));
            ActorMoved = new(this, nameof(ActorMoved));
            ActorTeleporting = new(this, nameof(ActorTeleporting));
            ActorSpawned = new(this, nameof(ActorSpawned));
            ActorDespawned = new(this, nameof(ActorDespawned));
            ActorWaited = new(this, nameof(ActorWaited));
            ActorDied = new(this, nameof(ActorDied));
            ActorKilled = new(this, nameof(ActorKilled));
            ActorAttacked = new(this, nameof(ActorAttacked));
            ActorDamaged = new(this, nameof(ActorDamaged));
            ActorHealed = new(this, nameof(ActorHealed));
            SpellLearned = new(this, nameof(SpellLearned));
            SpellForgotten = new(this, nameof(SpellForgotten));
            SpellCast = new(this, nameof(SpellCast));
            SpellTargeted = new(this, nameof(SpellTargeted));
            ItemPickedUp = new(this, nameof(ItemPickedUp));
            ItemDropped = new(this, nameof(ItemDropped));
            ItemThrown = new(this, nameof(ItemThrown));
            WandZapped = new(this, nameof(WandZapped));
            ScrollRead = new(this, nameof(ScrollRead));
            PotionQuaffed = new(this, nameof(PotionQuaffed));
            ItemEquipped = new(this, nameof(ItemEquipped));
            ItemUnequipped = new(this, nameof(ItemUnequipped));
            ItemConsumed = new(this, nameof(ItemConsumed));
            FeatureInteractedWith = new(this, nameof(FeatureInteractedWith));
            ExplosionHappened = new(this, nameof(ExplosionHappened));
            ActorSteppedOnTrap = new(this, nameof(ActorSteppedOnTrap));
            ActorBumpedObstacle = new(this, nameof(ActorBumpedObstacle));
            ActorGainedEffect = new(this, nameof(ActorGainedEffect));
            ActorLostEffect = new(this, nameof(ActorLostEffect));
            ActorIntentSelected = new(this, nameof(ActorIntentSelected));
            ActorIntentEvaluated = new(this, nameof(ActorIntentEvaluated));
            ActorIntentFailed = new(this, nameof(ActorIntentFailed));

            ActorAttacked.ResponseReceived += (_, e, r) => {
                if (r.All(x => x)) {
                    ActorDamaged.HandleOrThrow(new(e.Attacker, e.Victim, e.Weapon, e.Damage));
                }
            };
            ActorDamaged.ResponseReceived += (_, e, r) => {
                if (r.All(x => x)) {
                    if (e.Victim.ActorProperties.Health <= 0) {
                        if (e.Source.TryCast<Actor>(out var killer)) {
                            ActorKilled.HandleOrThrow(new(killer, e.Victim));
                        }
                        else {
                            ActorDied.HandleOrThrow(new(e.Victim));
                        }
                    }
                }
            };
            ActorKilled.ResponseReceived += (_, e, r) => {
                if (r.All(x => x)) {
                    ActorDied.HandleOrThrow(new(e.Victim));
                }
            };
            ActorDied.ResponseReceived += (_, e, r) => {
                if (r.All(x => x)) {
                    ActorDespawned.HandleOrThrow(new(e.Actor));
                }
            };
            Reset();
        }

        private int? HandleAction(ActorTime t, ref IAction action)
        {
            var ret = default(bool?);
            var cost = action.Cost;
            if (t.ActorId == TURN_ACTOR_ID)
                return cost;

            switch(action.Name) {
                case ActionName.Wait:
                    ret = HandleWait(t, ref action, ref cost);
                    break;
                case ActionName.Move:
                    ret = HandleMove(t, ref action, ref cost);
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
                case ActionName.Read:
                    ret = HandleReadScroll(t, ref action, ref cost);
                    break;
                case ActionName.Quaff:
                    ret = HandleQuaffPotion(t, ref action, ref cost);
                    break;
                case ActionName.Organize:
                    ret = HandleOrganize(t, ref action, ref cost);
                    break;
                case ActionName.Fail:
                    ret = false;
                    break;
            }
            t.Actor.Action.LastAction = action;
            if (ret == true) {
                ActorIntentEvaluated.Raise(new(t.Actor, action, CurrentTurn, t.Time));
            }
            else if(ret == false) {
                ActorIntentFailed.Raise(new(t.Actor, action, CurrentTurn, t.Time));
            }
            return cost;
        }
        
        public void Reset()
        {
            AbortCurrentTurn();
            _actorQueue.Clear();
            _actorQueue.Add(new ActorTime(TURN_ACTOR_ID, null, () => new WaitAction(), 0));
            GameStarted.Raise(new());
        }

        public void Spawn(Actor a)
        {
            ActorSpawned.Raise(new(a));
        }

        public void Despawn(Actor a)
        {
            ActorDespawned.Raise(new(a));
        }

        private bool MayTarget(Actor attacker, Actor victim)
        {
            return victim.IsAlive() && attacker.IsAffectedBy(EffectName.Confusion) || _factionSystem.GetRelationships(attacker, victim).Left.IsHostile();
        }

        private bool TryFindVictim(Coord p, Actor attacker, out Actor victim)
        {
            victim = default;
            var actorsHere = _floorSystem.GetActorsAt(attacker.FloorId(), p);
            if (!actorsHere.Any(a => MayTarget(attacker, a))) {
                return false;
            }
            victim = actorsHere.Single();
            return true;
        }

        private bool HandleAttack(AttackName type, Actor attacker, Actor victim, ref int? cost, Entity weapon, out int damage, out int swingDelay)
        {
            if (TryAttack(out damage, out swingDelay, type, attacker, victim, weapon)) {
                cost += swingDelay;
                return true;
            }
            return false;
        }

        public bool TryAttack(out int damage, out int swingDelay, AttackName type, Actor attacker, Actor victim, Entity attackWith)
        {
            damage = 1; swingDelay = 0;
            if(attackWith != null) {
                if (type == AttackName.Melee && attackWith.TryCast<Weapon>(out var w)) {
                    swingDelay = w.WeaponProperties.SwingDelay;
                    damage += w.WeaponProperties.BaseDamage;
                }
                else if (type == AttackName.Ranged && attackWith.TryCast<Throwable>(out var t)) {
                    damage += t.ThrowableProperties.BaseDamage;
                }
                else if (type == AttackName.Magic && attackWith.TryCast<Spell>(out var s)) {
                    swingDelay = s.SpellProperties.CastDelay;
                    damage += s.SpellProperties.BaseDamage;
                }
                else if (type == AttackName.Magic && attackWith.TryCast<Wand>(out _)) {
                    damage = 0;
                }
            }
            return ActorAttacked.Handle(new(type, attacker, victim, attackWith, damage, swingDelay));
        }

        public void Track(int actorId)
        {
            if (_actorQueue.Any(x => x.ActorId == actorId)) {
                return;
            }
            // Actors have their energy randomized when spawning, to distribute them better across turns
            var time = _actorQueue.Single(x => x.ActorId == TURN_ACTOR_ID).Time;
            var proxy = _entities.GetProxy<Actor>(actorId);
            var currentTurn = CurrentTurn;
            _actorQueue.Add(new ActorTime(actorId, proxy, () => {
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
            if(intent.Name != ActionName.None) {
                // Some effects might want to hook into this request to change the intent of an actor right before it's evaluated
                var altIntents = ActorIntentSelected.Request(new(next.Actor, intent, CurrentTurn, next.Time))
                    .Where(i => i.Result)
                    .OrderByDescending(i => i.Priority);
                if (altIntents.FirstOrDefault() is { } altIntent) {
                    intent = altIntent.NewIntent;
                }
            }
            if (HandleAction(next, ref intent) is { } cost) {
                if(_invalidate) {
                    _invalidate = false;
                    if(!_actorQueue.Any(a => a.ActorId == next.ActorId)) {
                        _actorQueue.Add(next);
                    }
                    return cost;
                }
                OnTurnEnded(next.ActorId);
            }
            else {
                _actorQueue.Insert(0, next);
                return null;
            }
            next = next.WithTime(next.Time + cost);
            if(next.ActorId == TURN_ACTOR_ID || next.Actor.IsAlive()) {
                var index = _actorQueue.FindIndex(t => t.Time > next.Time);
                if (index < 0) {
                    _actorQueue.Add(next);
                }
                else {
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
                if (actorId == TURN_ACTOR_ID) {
                    TurnStarted.Raise(new(++CurrentTurn));
                }
                else if(next.LastActedTime < next.Time) {
                    ActorTurnStarted.Raise(new(next.Actor, CurrentTurn, next.Time));
                }
            }

            void OnTurnEnded(int actorId)
            {
                if (actorId == TURN_ACTOR_ID) {
                    TurnEnded.Raise(new(CurrentTurn));
                }
                else {
                    ActorTurnEnded.Raise(new(next.Actor, CurrentTurn, next.Time));
                }
            }
        }

        public void Update(int playerId)
        {
            do {
                _entities.RemoveFlagged(true);
                ElapseTick();
            }
            while (ActorIds.Contains(playerId) && CurrentActorId != playerId);
        }
    }
}
