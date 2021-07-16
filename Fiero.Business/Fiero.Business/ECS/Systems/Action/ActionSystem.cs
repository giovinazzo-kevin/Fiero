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

    public partial class ActionSystem : EcsSystem
    {
        private const int TURN_ACTOR_ID = -1;

        private readonly List<ActorTime> _queue;
        public int CurrentTurn { get; private set; }

        private readonly GameEntities _entities;
        private readonly GameDataStore _store;
        private readonly GameInput _input;
        private readonly GameSounds<SoundName> _sounds;
        private readonly FloorSystem _floorSystem;

        public readonly SystemEvent<ActionSystem, TurnEvent> TurnStarted;
        public readonly SystemEvent<ActionSystem, TurnEvent> TurnEnded;
        public readonly SystemEvent<ActionSystem, ActorTurnEvent> ActorTurnStarted;
        public readonly SystemEvent<ActionSystem, ActorTurnEvent> ActorTurnEnded;

        public ActionSystem(
            EventBus bus,
            GameEntities entities, 
            FloorSystem floorSystem,
            GameDataStore store, 
            GameInput input,
            GameSounds<SoundName> sounds
        ) : base(bus) {
            _entities = entities;
            _floorSystem = floorSystem;
            _sounds = sounds;
            _store = store;
            _input = input;
            _queue = new List<ActorTime>();
            Clear();

            TurnStarted = new(this, nameof(TurnStarted));
            TurnEnded = new(this, nameof(TurnEnded));
            ActorTurnStarted = new(this, nameof(ActorTurnStarted));
            ActorTurnEnded = new(this, nameof(ActorTurnEnded));
        }

        protected virtual int? HandleAction(Actor actor, ref IAction action)
        {
            var cost = action.Cost;
            if (actor is null)
                return cost;
            cost = action.Name switch {
                ActionName.Wait     when(HandleWait    (actor, ref action, ref cost)) => cost,
                ActionName.Move     when(HandleMove    (actor, ref action, ref cost)) => cost,
                ActionName.Attack   when(HandleAttack  (actor, ref action, ref cost)) => cost,
                ActionName.Interact when(HandleInteract(actor, ref action, ref cost)) => cost,
                ActionName.Organize when(HandleOrganize(actor, ref action, ref cost)) => cost,
                _ => null
            };
            actor.Action.LastAction = action;
            return cost;
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

        public void Clear()
        {
            _queue.Clear();
            _queue.Add(new ActorTime(TURN_ACTOR_ID, null, () => new WaitAction(), 0));
        }

        private ActorTime Dequeue()
        {
            var next = _queue[0];
            _queue.RemoveAt(0);
            return next;
        }

        public int? ElapseTick()
        {
            var next = Dequeue();
            var isTurnCounter = next.ActorId == TURN_ACTOR_ID;
            if (isTurnCounter) {
                TurnStarted.Raise(new(++CurrentTurn));
            }
            else {
                ActorTurnStarted.Raise(new(next.Proxy, CurrentTurn));
            }
            var intent = next.GetIntent();
            if (HandleAction(next.Proxy, ref intent) is { } cost) {
                if (isTurnCounter) {
                    TurnEnded.Raise(new(CurrentTurn));
                }
                else {
                    ActorTurnEnded.Raise(new(next.Proxy, CurrentTurn));
                }
            }
            else {
                _queue.Insert(0, next);
                return null;
            }
            next = next.WithTime(next.Time + cost);
            var index = _queue.FindIndex(0, t => t.Time >= next.Time);
            if (index > -1) {
                _queue.Insert(index, next);
            }
            else {
                _queue.Add(next);
            }
            return cost;
        }

        public void Update()
        {
            ElapseTick();
        }
    }
}
