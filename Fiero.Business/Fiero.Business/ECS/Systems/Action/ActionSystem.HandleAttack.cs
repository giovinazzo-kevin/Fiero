﻿using Fiero.Core;
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
                return CanTargetVictim() && HandleMeleeAttack(ref cost);
            }
            else if (action is MeleeAttackPointAction dir) {
                var newPos = t.Actor.Physics.Position + dir.Point;
                return TryFindVictim(newPos, out victim) && CanTargetVictim() && HandleMeleeAttack(ref cost);
            }
            else if (action is RangedAttackOtherAction rOth) {
                victim = rOth.Victim;
                return CanTargetVictim() && HandleRangedAttack(ref cost);
            }
            else if (action is RangedAttackPointAction rDir) {
                // the point is relative to the actor's position
                var newPos = t.Actor.Physics.Position + rDir.Point;
                return TryFindVictim(newPos, out victim) && CanTargetVictim() && HandleRangedAttack(ref cost);
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

            bool CanTargetVictim()
            {
                return t.Actor.Faction.Relationships.Get(victim.Faction.Type).MayAttack();
            }

            bool HandleMeleeAttack(ref int? cost)
            {
                if (t.Actor.DistanceFrom(victim) >= 2) {
                    // out of reach
                    return false;
                }
                return HandleAttack(AttackName.Melee, ref cost);
            }

            bool HandleRangedAttack(ref int? cost)
            {
                if (_floorSystem.IsLineOfSightBlocked(t.Actor.FloorId(), t.Actor.Physics.Position, victim.Physics.Position)) {
                    return false;
                }
                // TODO: Check for weapon max range
                return HandleAttack(AttackName.Ranged, ref cost);
            }

            bool HandleAttack(AttackName type, ref int? cost)
            {
                // attack!
                var attackResponse = ActorAttacked.Request(new(type, t.Actor, victim)).First(x => x);
                if (!attackResponse) {
                    return false;
                }
                cost += attackResponse.AdditionalCost;
                if (victim.ActorProperties.Stats.Health <= 0) {
                    RemoveActor(victim.Id);
                    return ActorKilled.Request(new(t.Actor, victim)).All(x => x);
                }
                return true;
            }
        }
    }
}
