using Fiero.Core;
using System;
using System.Linq;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleMeleeAttack(ActorTime t, ref IAction action, ref int? cost)
        {
            var victim = default(Actor);
            if (action is MeleeAttackOtherAction oth) {
                victim = oth.Victim;
                return MayTarget(t.Actor, victim) 
                    && HandleMeleeAttack(ref cost, oth.Weapon);
            }
            else if (action is MeleeAttackPointAction dir) {
                var newPos = t.Actor.Position() + dir.Point;
                return TryFindVictim(newPos, t.Actor, out victim) 
                    && MayTarget(t.Actor, victim)
                    && HandleMeleeAttack(ref cost, dir.Weapon);
            }
            else throw new NotSupportedException(action.GetType().Name);

            bool HandleMeleeAttack(ref int? cost, Weapon weapon)
            {
                if (t.Actor.DistanceFrom(victim) >= 2) {
                    // out of reach
                    return false;
                }
                return HandleAttack(AttackName.Melee, t.Actor, victim, ref cost, weapon, out _, out _);
            }
        }
    }
}
