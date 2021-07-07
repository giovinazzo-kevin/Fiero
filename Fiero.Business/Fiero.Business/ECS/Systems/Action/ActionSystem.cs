using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace Fiero.Business
{

    public sealed class ActionSystem
    {
        internal readonly struct ActorTime
        {
            public readonly int ActorId;
            public readonly int Time;
            public readonly Func<int?> Act;

            public ActorTime(int actorId, Func<int?> actionCost, int time = 0)
            {
                Time = time;
                ActorId = actorId;
                Act = actionCost;
            }

            public ActorTime WithTime(int newTime) => new(ActorId, Act, newTime);
        }

        private const int TURN_ACTOR_ID = -1;

        private readonly List<ActorTime> _queue;
        public int CurrentTime { get; private set; }
        public int CurrentTurn { get; private set; }

        private readonly GameEntities _entities;
        private readonly GameDataStore _store;
        private readonly GameSounds<SoundName> _sounds;
        private readonly FloorSystem _floorSystem;

        public event Action<int> TurnStarted;
        public event Action<int> PlayerTurnStarted;

        public ActionSystem(GameEntities entities, FloorSystem floorSystem, GameDataStore store, GameSounds<SoundName> sounds)
        {
            _entities = entities;
            _floorSystem = floorSystem;
            _sounds = sounds;
            _store = store;
            _queue = new List<ActorTime>();
            Clear();
        }

        private int? HandleAction(int actorId)
        {
            var actor = _entities.GetProxy<Actor>(actorId);
            var action = actor.Action.GetAction();
            
            if (action == ActionName.Move && HandleMove()) {
                return GetCost(action);
            }
            if(action == ActionName.Attack && HandleAttack()) {
                return GetCost(action);
            }
            if (action == ActionName.Use && HandleUse()) {
                return GetCost(action);
            }
            return GetCost(action);

            int? GetCost(ActionName action)
            {

                return action switch {
                    ActionName.None => default(int?),
                    ActionName.Use => 25,
                    ActionName.Attack => 100,
                    ActionName.Move => 100,
                    _ => 0
                };
            }

            bool HandleUse()
            {
                // Use handles both grabbing items from the ground and using dungeon features
                if (!(actor.Action.Direction is { } direction)) {
                    return true;
                }
                var usePos = actor.Physics.Position + direction;
                var itemsHere = _floorSystem.ItemsAt(usePos);
                var featuresHere = _floorSystem.FeaturesAt(usePos);
                if(itemsHere.Any() && actor.Inventory != null) {
                    var item = itemsHere.Single();
                    if(actor.Inventory.TryPut(item)) {
                        _floorSystem.CurrentFloor.RemoveItem(item.Id);
                        actor.Log?.Write($"$Action.YouPickUpA$ {item.DisplayName}.");
                    }
                    else {
                        actor.Log?.Write($"$Action.YourInventoryIsTooFullFor$ {item.DisplayName}.");
                    }
                }
                else if(featuresHere.Any()) {
                    var feature = featuresHere.Single();
                    return HandleUseFeature(feature);
                }
                return false;

                bool HandleUseFeature(Feature feature)
                {
                    if (feature.Properties.Type == FeatureName.Shrine) {
                        actor.Log?.Write($"$Action.YouKneelAt$ {feature.Info.Name}.");
                    }
                    if (feature.Properties.Type == FeatureName.Chest) {
                        actor.Log?.Write($"$Action.YouOpen$ {feature.Info.Name}.");
                    }
                    return false;
                }
            }

            bool HandleAttack()
            {
                if (actor.Action.Target == null) {
                    if (!(actor.Action.Direction is { } direction)) {
                        return true;
                    }
                    var newPos = actor.Physics.Position + direction;
                    var actorsHere = _floorSystem.ActorsAt(newPos);
                    if (!actorsHere.Any(a => actor.Faction.Relationships.Get(a.Faction.Type).MayAttack())) {
                        return true;
                    }
                    actor.Action.Target = actorsHere.Single();
                }
                if (actor.DistanceFrom(actor.Action.Target) >= 2) {
                    // out of reach
                    return true;
                }
                if (actor.Faction.Relationships.Get(actor.Action.Target.Faction.Type).MayAttack()) {
                    // attack!
                    actor.Log?.Write($"$Action.YouAttack$ {actor.Action.Target.Info.Name}.");
                    actor.Action.Target.Log?.Write($"{actor.Info.Name} $Action.AttacksYou$.");
                    // make sure that neutrals aggro the attacker
                    if(actor.Action.Target.Action.Target == null) {
                        actor.Action.Target.Action.Target = actor;
                    }
                    // make sure that people hold a grudge regardless of factions
                    actor.Action.Target.ActorProperties.Relationships.TryUpdate(actor, x => x
                        .With(StandingName.Hated)
                    , out _);

                    if (--actor.Action.Target.ActorProperties.Health <= 0) {
                        actor.Action.Target.Log?.Write($"{actor.Info.Name} $Action.KillsYou$.");
                        actor.Log?.Write($"$Action.YouKill$ {actor.Action.Target.Info.Name}.");
                        if(actor.Action.Target.ActorProperties.Type == ActorName.Player) {
                            _sounds.Get(SoundName.PlayerDeath).Play();
                            _store.SetValue(Data.Player.KilledBy, actor);
                        }
                        RemoveActor(actor.Action.Target.Id);
                        _floorSystem.CurrentFloor.RemoveActor(actor.Action.Target.Id);
                        _floorSystem.CurrentFloor.Entities.FlagEntityForRemoval(actor.Action.Target.Id);
                        actor.Action.Target.TryRefresh(0); // invalidate target proxy
                    }
                }
                else {
                    // friendly fire?
                }

                return false;
            }

            bool HandleMove()
            {
                var direction = actor.Action.Direction
                    ?? (actor.Action.Target != null
                        ? new(actor.Action.Target.Physics.Position.X - actor.Physics.Position.X,
                              actor.Action.Target.Physics.Position.Y - actor.Physics.Position.Y)
                        : new(Rng.Random.Next(-1, 2), Rng.Random.Next(-1, 2)));
                var newPos = new Coord(actor.Physics.Position.X + direction.X, actor.Physics.Position.Y + direction.Y);
                if (newPos == actor.Physics.Position) {
                    actor.ActorProperties.Health = Math.Min(actor.ActorProperties.MaximumHealth, actor.ActorProperties.Health + 1);
                    return true; // waiting costs the same as moving
                }
                if (_floorSystem.TileAt(newPos, out var tile)) {
                    if (tile.TileProperties.Name == TileName.Door) {
                        _floorSystem.UpdateTile(newPos, TileName.Ground);
                        actor.Log?.Write("$Action.YouOpenTheDoor$.");
                    }
                    else if (!tile.TileProperties.BlocksMovement) {
                        var actorsHere = _floorSystem.ActorsAt(newPos);
                        var featuresHere = _floorSystem.FeaturesAt(newPos);
                        if (!actorsHere.Any()) {
                            if (!featuresHere.Any(f => f.Properties.BlocksMovement)) {
                                actor.Physics.Position = newPos;
                            }
                            else {
                                var feature = featuresHere.Single();
                                actor.Action.Direction = new(
                                    feature.Physics.Position.X - actor.Physics.Position.X,
                                    feature.Physics.Position.Y - actor.Physics.Position.Y
                                );
                                action = ActionName.Use; // you can bump shrines and chests to interact with them
                            }
                        }
                        else {
                            var target = actorsHere.Single();
                            if (actor.IsHotileTowards(target)) {
                                actor.Action.Target = target;
                                action = ActionName.Attack; // attack-bump is a free "combo"
                            }
                        }
                    }
                    else {
                        actor.Log?.Write("$Action.YouBumpIntoTheWall$.");
                        if (actor.ActorProperties.Type == ActorName.Player) {
                            _sounds.Get(SoundName.WallBump).Play();
                        }
                    }
                }
                return false;
            }
        }

        public void AddActor(int actorId)
        {
            // Actors have their energy randomized when spawning, to distribute them better across turns
            var time = _queue.Single(x => x.ActorId == TURN_ACTOR_ID).Time;
            _queue.Add(new ActorTime(actorId, () => HandleAction(actorId), time + Rng.Random.Next(0, 100)));
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
                PlayerTurnStarted?.Invoke(CurrentTurn);
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
                TurnStarted?.Invoke(CurrentTurn);
            }

            return cost;
        }
    }
}
