using Fiero.Core;
using System;
using System.Linq;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleMove(ActorTime t, ref IAction action, ref int? cost)
        {
            if (t.Actor.IsImmobile())
            {
                action = new WaitAction();
                cost = HandleAction(t, ref action);
                return true;
            }
            var direction = default(Coord);
            if (action is MoveRelativeAction rel)
                direction = rel.Coord;
            else if (action is MoveRandomlyAction ran)
                direction = new(Rng.Random.Next(-1, 2), Rng.Random.Next(-1, 2));
            else throw new NotSupportedException();

            var floorId = t.Actor.FloorId();
            var oldPos = t.Actor.Position();
            var newPos = t.Actor.Position() + direction;
            if (newPos == t.Actor.Position())
            {
                action = new WaitAction();
                cost = HandleAction(t, ref action);
            }
            else if (_floorSystem.TryGetTileAt(floorId, newPos, out var tile))
            {
                if (t.Actor.Physics.Phasing || !tile.Physics.BlocksMovement)
                {
                    var actorsHere = _floorSystem.GetActorsAt(floorId, newPos);
                    var featuresHere = _floorSystem.GetFeaturesAt(floorId, newPos);
                    if (!actorsHere.Any())
                    {
                        if (t.Actor.Physics.Phasing || !featuresHere.Any(f => f.Physics.BlocksMovement))
                        {
                            return ActorMoved.Handle(new(t.Actor, oldPos, newPos));
                        }
                        else
                        {
                            var feature = featuresHere.Single();
                            ActorBumpedObstacle.Raise(new(t.Actor, feature));
                            // you can bump shrines and chests to interact with them
                            action = new InteractWithFeatureAction(feature);
                            cost = HandleAction(t, ref action);
                        }
                    }
                    else
                    {
                        var target = actorsHere.Single();
                        var relationship = _factionSystem.GetRelations(t.Actor, target).Left;
                        if (relationship.MayAttack())
                        {
                            // attack-bump is a free "combo"
                            action = new MeleeAttackOtherAction(target, t.Actor.Equipment.Weapon);
                            cost = HandleAction(t, ref action);
                        }
                        else if (relationship.IsFriendly())
                        {
                            // you can swap position with allies in twice the amount of time it takes to move
                            cost *= 2;
                            return ActorMoved.Handle(new(t.Actor, oldPos, newPos))
                                   && ActorMoved.Handle(new(target, newPos, oldPos));
                        }
                    }
                }
                else
                {
                    ActorBumpedObstacle.Raise(new(t.Actor, tile));
                    return false;
                }
            }
            else
            {
                // Bumped "nothingness"
                ActorBumpedObstacle.Raise(new(t.Actor, null));
                return false;
            }
            return true;
        }
    }
}
