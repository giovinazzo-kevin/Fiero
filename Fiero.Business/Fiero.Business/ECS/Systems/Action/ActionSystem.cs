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

        public readonly SystemEvent<ActionSystem, TurnStartedEvent> TurnStarted;
        public readonly SystemEvent<ActionSystem, PlayerTurnStartedEvent> PlayerTurnStarted;

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
            PlayerTurnStarted = new(this, nameof(PlayerTurnStarted));
        }

        protected virtual int? GetCost(ActionName action)
        {

            return action switch {
                ActionName.None => default(int?),
                ActionName.Interact => 25,
                ActionName.Attack => 100,
                ActionName.Move => 100,
                _ => 0
            };
        }

        protected virtual int? HandleAction(Actor actor, ref IAction action)
        {
            var cost = GetCost(action.Name);
            cost = action.Name switch {
                ActionName.Move     when(HandleMove  (actor, ref action, ref cost)) => cost,
                ActionName.Attack   when(HandleAttack(actor, ref action, ref cost)) => cost,
                ActionName.Interact when(HandleInteract   (actor, ref action, ref cost)) => cost,
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
            _queue.Add(new ActorTime(actorId, () => {
                var action = proxy.Action.ActionProvider.GetIntent(proxy);
                return HandleAction(proxy, ref action);
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

            if(next.ActorId != TURN_ACTOR_ID && _entities.TryGetFirstComponent<ActorComponent>(next.ActorId, out var comp) 
                && comp.Type == ActorName.Player) {
                PlayerTurnStarted.Raise(new(next.ActorId, CurrentTurn));
            }

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
                CurrentTurn++;
                TurnStarted.Raise(new(CurrentTurn));
            }

            return cost;
        }
    }
}
