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
            if (action is MeleeAttackOtherAction oth) {
                victim = oth.Victim;
                return HandleMeleeAttack();
            }
            else if (action is MeleeAttackPointAction dir) {
                var newPos = t.Actor.Physics.Position + dir.Point;
                return TryFindVictim(newPos, out victim) && HandleMeleeAttack();
            }
            else if (action is RangedAttackOtherAction rOth) {
                victim = rOth.Victim;
                return HandleRangedAttack();
            }
            else if (action is RangedAttackPointAction rDir) {
                // the point is relative to the actor's position
                var newPos = t.Actor.Physics.Position + rDir.Point;
                return TryFindVictim(newPos, out victim) && HandleRangedAttack();
            }
            else throw new NotSupportedException(action.GetType().Name);

            bool TryFindVictim(Coord p, out Actor victim)
            {
                victim = default;
                var actorsHere = _floorSystem.GetActorsAt(t.Actor.FloorId(), p);
                if (!actorsHere.Any(a => t.Actor.Faction.Relationships.Get(a.Faction.Type).MayAttack())) {
                    return false;
                }
                victim = actorsHere.Single();
                return true;
            }

            bool HandleMeleeAttack()
            {
                if (t.Actor.DistanceFrom(victim) >= 2) {
                    // out of reach
                    return false;
                }
                return HandleAttack(AttackName.Melee);
            }

            bool HandleRangedAttack()
            {
                if(_floorSystem.IsLineOfSightBlocked(t.Actor.FloorId(), t.Actor.Physics.Position, victim.Physics.Position)) {
                    return false;
                }
                // TODO: Check for weapon max range
                return HandleAttack(AttackName.Ranged);
            }

            bool HandleAttack(AttackName type)
            {
                if (t.Actor.Faction.Relationships.Get(victim.Faction.Type).MayAttack()) {
                    // attack!
                    if (!ActorAttacked.Request(new(type, t.Actor, victim)).All(x => x)) {
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
}
