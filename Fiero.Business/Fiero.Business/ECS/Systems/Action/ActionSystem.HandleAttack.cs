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
        private bool HandleAttack(ActorTime t, ref IAction action, ref int? cost)
        {
            var victim = default(Actor);
            if (action is AttackOtherAction oth)
                victim = oth.Victim;
            else if (action is AttackDirectionAction dir) {
                var newPos = t.Actor.Physics.Position + dir.Coord;
                var actorsHere = _floorSystem.GetActorsAt(t.Actor.FloorId(), newPos);
                if (!actorsHere.Any(a => t.Actor.Faction.Relationships.Get(a.Faction.Type).MayAttack())) {
                    return false;
                }
                victim = actorsHere.Single();
            }
            else throw new NotSupportedException(action.GetType().Name);
            if (t.Actor.DistanceFrom(victim) >= 2) {
                // out of reach
                return false;
            }
            if (t.Actor.Faction.Relationships.Get(victim.Faction.Type).MayAttack()) {
                // attack!
                if(!ActorAttacked.Request(new(t.Actor, victim)).All(x => x)) {
                    return false;
                }
                if (victim.ActorProperties.Health <= 0) {
                    RemoveActor(victim.Id);
                    return ActorKilled.Request(new(t.Actor, victim)).All(x => x);
                }
            }
            else {
                // TODO: friendly fire?
                return false;
            }
            return true;
        }
    }
}
