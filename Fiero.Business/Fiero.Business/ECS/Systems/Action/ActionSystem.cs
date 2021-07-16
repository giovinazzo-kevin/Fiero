using Fiero.Core;
using SFML.Graphics;
using SFML.System;
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
        public int CurrentTime { get; private set; }
        public int CurrentTurn { get; private set; }

        private readonly GameEntities _entities;
        private readonly GameDataStore _store;
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
            GameSounds<SoundName> sounds
        ) : base(bus) {
            _entities = entities;
            _floorSystem = floorSystem;
            _sounds = sounds;
            _store = store;
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
            cost = action.Name switch {
                ActionName.Wait                                                       => cost,
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
            _queue.Add(new ActorTime(actorId, () => {
                if(currentTurn != CurrentTurn) {
                    ActorTurnStarted.Raise(new(proxy, currentTurn = CurrentTurn));
                }
                var action = proxy.Action.ActionProvider.GetIntent(proxy);
                if(HandleAction(proxy, ref action) is { } cost) {
                    ActorTurnEnded.Raise(new(proxy, currentTurn));
                    return cost;
                }
                return null;
            }, time + Rng.Random.Next(0, 100)));
        }

        public void RemoveActor(int actorId)
        {
            _queue.RemoveAll(x => x.ActorId == actorId);
        }

        public void Clear()
        {
            _queue.Clear();
            _queue.Add(new ActorTime(TURN_ACTOR_ID, () => 100, 0));
            CurrentTime = 0;
        }

        public int? Update()
        {
            var next = _queue[0];
            _queue.RemoveAt(0);

            var cost = next.Act();
            if (!cost.HasValue) {
                _queue.Insert(0, next);
                return null;
            }

            next = next.WithTime(next.Time + cost.Value);

            var index = _queue.FindIndex(0, t => t.Time >= next.Time);
            if (index > -1) {
                _queue.Insert(index, next);
            }
            else {
                _queue.Add(next);
            }

            CurrentTime += cost.Value;

            if (next.ActorId == TURN_ACTOR_ID) {
                TurnEnded.Raise(new(CurrentTurn));
                TurnStarted.Raise(new(++CurrentTurn));
            }

            return cost;
        }
    }
}
