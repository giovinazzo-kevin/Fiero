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

        private readonly List<ActorTime> _queue;
        public int CurrentTurn { get; private set; }

        private readonly GameEntities _entities;
        private readonly GameSounds<SoundName> _sounds;
        private readonly FloorSystem _floorSystem;

        public readonly SystemRequest<ActionSystem, TurnEvent, bool> GameStarted;
        public readonly SystemRequest<ActionSystem, TurnEvent, bool> TurnStarted;
        public readonly SystemRequest<ActionSystem, TurnEvent, bool> TurnEnded;
        public readonly SystemRequest<ActionSystem, ActorTurnEvent, bool> ActorTurnStarted;
        public readonly SystemRequest<ActionSystem, ActorTurnEvent, bool> ActorTurnEnded;
        public readonly SystemRequest<ActionSystem, ActorMovedEvent, bool> ActorMoved;
        public readonly SystemRequest<ActionSystem, ActorSpawnedEvent, bool> ActorSpawned;
        public readonly SystemRequest<ActionSystem, ActorDiedEvent, bool> ActorDied;
        public readonly SystemRequest<ActionSystem, ActorKilledEvent, bool> ActorKilled;
        public readonly SystemRequest<ActionSystem, ActorAttackedEvent, bool> ActorAttacked;
        public readonly SystemRequest<ActionSystem, ItemPickedUpEvent, bool> ItemPickedUp;
        public readonly SystemRequest<ActionSystem, ItemDroppedEvent, bool> ItemDropped;
        public readonly SystemRequest<ActionSystem, ItemDroppedEvent, bool> ItemEquipped;
        public readonly SystemRequest<ActionSystem, ItemDroppedEvent, bool> ItemUnequipped;
        public readonly SystemRequest<ActionSystem, ItemDroppedEvent, bool> ItemConsumed;
        public readonly SystemRequest<ActionSystem, FeatureInteractedWithEvent, bool> FeatureInteractedWith;

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
            var intent = next.GetIntent();
            if (HandleAction(next, ref intent) is { } cost) {
                OnTurnEnded(next.ActorId);
            }
            else {
                _queue.Insert(0, next);
                return null;
            }
            next = next.WithTime(next.Time + cost);
            _queue.Add(next);
            return cost;

            ActorTime Dequeue()
            {
                var next = _queue[0];
                _queue.RemoveAt(0);
                return next;
            }

            void OnTurnStarted(int actorId)
            {
                if (actorId == TURN_ACTOR_ID)
                    TurnStarted.Raise(new(++CurrentTurn));
                else
                    ActorTurnStarted.Raise(new(next.Actor, CurrentTurn));
            }

            void OnTurnEnded(int actorId)
            {
                if (actorId == TURN_ACTOR_ID)
                    TurnEnded.Raise(new(CurrentTurn));
                else
                    ActorTurnEnded.Raise(new(next.Actor, CurrentTurn));
            }
        }

        public void Update()
        {
            ElapseTick();
        }
    }
}
