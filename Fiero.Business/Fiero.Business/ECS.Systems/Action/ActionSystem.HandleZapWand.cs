using Fiero.Core;
using System;
using System.Linq;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleZapWand(ActorTime t, ref IAction action, ref int? cost)
        {
            if (action is ZapWandAtOtherAction rOth) {
                return ItemConsumed.Handle(new(t.Actor, rOth.Wand)) 
                    && HandleMagicAttack(t.Actor, rOth.Victim, ref cost, rOth.Wand);
            }
            else if (action is ZapWandAtPointAction rDir) {
                // the point is relative to the actor's position
                var newPos = t.Actor.Position() + rDir.Point;
                if(TryFindVictim(newPos, t.Actor, out var victim)) {
                    return ItemConsumed.Handle(new(t.Actor, rDir.Wand))
                        && HandleMagicAttack(t.Actor, victim, ref cost, rDir.Wand);
                }
                else {
                    return ItemConsumed.Handle(new(t.Actor, rDir.Wand)) 
                        && WandZapped.Handle(new(t.Actor, null, newPos, rDir.Wand));
                }
            }
            else throw new NotSupportedException(action.GetType().Name);

            bool HandleMagicAttack(Actor attacker, Actor victim, ref int? cost, Wand item)
            {
                var vPos = victim.Position();
                return WandZapped.Handle(new(attacker, victim, vPos, item)) 
                    && HandleAttack(AttackName.Magic, t.Actor, victim, ref cost, item, out _, out _);
            }
        }
    }
}
