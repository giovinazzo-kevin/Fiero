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

    public readonly struct EventResult
    {
        public readonly bool Cancel;
        public readonly int AdditionalCost;

        public EventResult(bool cancel, int additionalCost = 0)
        {
            Cancel = cancel;
            AdditionalCost = additionalCost;
        }


        public static implicit operator bool(EventResult r) => r.Cancel;
        public static implicit operator EventResult(bool b) => new(b);

    }

    public sealed partial class ActionSystem : EcsSystem
    {
        private const int TURN_ACTOR_ID = -1;

        private readonly List<ActorTime> _queue;
        public int CurrentTurn { get; private set; }

        private readonly GameEntities _entities;
        private readonly GameSounds<SoundName> _sounds;
        private readonly FloorSystem _floorSystem;

        public readonly SystemRequest<ActionSystem, TurnEvent, EventResult> GameStarted;
        public readonly SystemRequest<ActionSystem, TurnEvent, EventResult> TurnStarted;
        public readonly SystemRequest<ActionSystem, TurnEvent, EventResult> TurnEnded;
        public readonly SystemRequest<ActionSystem, ActorTurnEvent, EventResult> ActorTurnStarted;
        public readonly SystemRequest<ActionSystem, ActorTurnEvent, EventResult> ActorTurnEnded;
        public readonly SystemRequest<ActionSystem, ActorMovedEvent, EventResult> ActorMoved;
        public readonly SystemRequest<ActionSystem, ActorSpawnedEvent, EventResult> ActorSpawned;
        public readonly SystemRequest<ActionSystem, ActorDiedEvent, EventResult> ActorDied;
        public readonly SystemRequest<ActionSystem, ActorKilledEvent, EventResult> ActorKilled;
        public readonly SystemRequest<ActionSystem, ActorAttackedEvent, EventResult> ActorAttacked;
        public readonly SystemRequest<ActionSystem, ItemPickedUpEvent, EventResult> ItemPickedUp;
        public readonly SystemRequest<ActionSystem, ItemDroppedEvent, EventResult> ItemDropped;
        public readonly SystemRequest<ActionSystem, ItemDroppedEvent, EventResult> ItemEquipped;
        public readonly SystemRequest<ActionSystem, ItemDroppedEvent, EventResult> ItemUnequipped;
        public readonly SystemRequest<ActionSystem, ItemDroppedEvent, EventResult> ItemConsumed;
        public readonly SystemRequest<ActionSystem, FeatureInteractedWithEvent, EventResult> FeatureInteractedWith;

        public readonly SystemEvent<ActionSystem, ActorTurnEvent> ActorIntentEvaluated;

        public int CurrentActorId => _queue[0].ActorId;
        public IEnumerable<int> ActorIds => _queue.Select(x => x.ActorId);

        public ActionSystem(
            EventBus bus,
            GameEntities entities, 
            FloorSystem floorSystem,
            GameSounds<SoundName> sounds
        ) : base(bus) {
            _entities = entities;
            _floorSystem = floorSystem;
            _sounds = sounds;
            _queue = new List<ActorTime>();

            GameStarted = new(this, nameof(GameStarted));
            TurnStarted = new(this, nameof(TurnStarted));
            TurnEnded = new(this, nameof(TurnEnded));
            ActorTurnStarted = new(this, nameof(ActorTurnStarted));
            ActorIntentEvaluated = new(this, nameof(ActorIntentEvaluated));
            ActorTurnEnded = new(this, nameof(ActorTurnEnded));
            ActorMoved = new(this, nameof(ActorMoved));
            ActorSpawned = new(this, nameof(ActorSpawned));
            ActorDied = new(this, nameof(ActorDied));
            ActorKilled = new(this, nameof(ActorKilled));
            ActorAttacked = new(this, nameof(ActorAttacked));
            ItemPickedUp = new(this, nameof(ItemPickedUp));
            ItemDropped = new(this, nameof(ItemDropped));
            ItemEquipped = new(this, nameof(ItemEquipped));
            ItemUnequipped = new(this, nameof(ItemUnequipped));
            ItemConsumed = new(this, nameof(ItemConsumed));
            FeatureInteractedWith = new(this, nameof(FeatureInteractedWith));

            ActorKilled.SubscribeHandler(e => ActorDied.Raise(new(e.Victim)));
            Reset();
        }

        private int? HandleAction(ActorTime t, ref IAction action)
        {
            var cost = action.Cost;
            if (t.ActorId == TURN_ACTOR_ID)
                return cost;
            cost = action.Name switch {
                ActionName.Wait     when(HandleWait    (t, ref action, ref cost)) => cost,
                ActionName.Move     when(HandleMove    (t, ref action, ref cost)) => cost,
                ActionName.Attack   when(HandleAttack  (t, ref action, ref cost)) => cost,
                ActionName.Interact when(HandleInteract(t, ref action, ref cost)) => cost,
                ActionName.Organize when(HandleOrganize(t, ref action, ref cost)) => cost,
                _ => null
            };
            t.Actor.Action.LastAction = action;
            if(action.Name != ActionName.None) {
                ActorIntentEvaluated.Raise(new(t.Actor, CurrentTurn, t.Time));
            }
            return cost;
        }
        
        public void Reset()
        {
            _queue.Clear();
            _queue.Add(new ActorTime(TURN_ACTOR_ID, null, () => new WaitAction(), 0));
            GameStarted.Raise(new());
        }


        public void AddActor(int actorId)
        {
            // Actors have their energy randomized when spawning, to distribute them better across turns
            var time = _queue.Single(x => x.ActorId == TURN_ACTOR_ID).Time;
            var proxy = _entities.GetProxy<Actor>(actorId);
            var currentTurn = CurrentTurn;
            _queue.Add(new ActorTime(actorId, proxy, () => {
                return proxy.Action.ActionProvider.GetIntent(proxy);
            }, time + Rng.Random.Next(0, 100)));
        }

        public void RemoveActor(int actorId)
        {
            _queue.RemoveAll(x => x.ActorId == actorId);
        }

        public int? ElapseTick()
        {
            var next = Dequeue();
            OnTurnStarted(next.ActorId);
            next = next.WithLastActedTime(next.Time);
            var intent = next.GetIntent();
            if (HandleAction(next, ref intent) is { } cost) {
                OnTurnEnded(next.ActorId);
            }
            else {
                _queue.Insert(0, next);
                return null;
            }
            next = next.WithTime(next.Time + cost);
            var index = _queue.FindIndex(t => t.Time > next.Time);
            if(index < 0) {
                _queue.Add(next);
            }
            else {
                _queue.Insert(index, next);
            }
            return cost;

            ActorTime Dequeue()
            {
                var next = _queue[0];
                _queue.RemoveAt(0);
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
                if (actorId == TURN_ACTOR_ID)
                    TurnEnded.Raise(new(CurrentTurn));
                else
                    ActorTurnEnded.Raise(new(next.Actor, CurrentTurn, next.Time));
            }
        }

        public void Update()
        {
            ElapseTick();
        }
    }
}
