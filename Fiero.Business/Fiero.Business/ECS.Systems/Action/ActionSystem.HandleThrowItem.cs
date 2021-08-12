using Fiero.Core;
using System;
using System.Linq;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {

        private bool HandleThrowItem(ActorTime t, ref IAction action, ref int? cost)
        {
            if (action is ThrowItemAtOtherAction rOth) {
                return MayTarget(t.Actor, rOth.Victim)
                    && HandleRangedAttack(t.Actor, rOth.Victim, ref cost, rOth.Item);
            }
            else if (action is ThrowItemAtPointAction rDir) {
                // the point is relative to the actor's position
                var newPos = t.Actor.Position() + rDir.Point;
                if(TryFindVictim(newPos, t.Actor, out var victim) && MayTarget(t.Actor, victim)) {
                    return HandleRangedAttack(t.Actor, victim, ref cost, rDir.Item);
                }
                else {
                    return ItemThrown.Handle(new(t.Actor, null, newPos, rDir.Item));
                }
            }
            else throw new NotSupportedException(action.GetType().Name);

            bool HandleRangedAttack(Actor attacker, Actor victim, ref int? cost, Throwable item)
            {
                var floorId = attacker.FloorId();
                var aPos = attacker.Position();
                var vPos = victim.Position();
                if(aPos.Dist(vPos) > item.ThrowableProperties.MaximumRange + 1) {
                    return false;
                }
                switch(item.ThrowableProperties.Throw) {
                    case { } when _floorSystem.IsLineOfSightBlocked(floorId, aPos, vPos):
                        return false;
                    // If the throwable hits in a straight line, we need to figure out the first actor across that line
                    case ThrowName.Line:
                        victim = Shapes.Line(aPos, vPos)
                            .SelectMany(p => _floorSystem.GetActorsAt(floorId, p))
                            .Except(new[] { attacker })
                            .First();
                        break;
                }
                return ItemThrown.Handle(new(t.Actor, victim, vPos, item)) 
                    && HandleAttack(AttackName.Ranged, t.Actor, victim, ref cost, item, out _, out _);
            }

        }
    }
}
