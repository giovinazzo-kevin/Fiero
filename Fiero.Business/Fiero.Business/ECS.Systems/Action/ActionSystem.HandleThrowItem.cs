using Fiero.Core;
using System;
using System.Linq;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleThrowItem(ActorTime t, ref IAction action, ref int? cost)
        {
            if (action is ThrowItemAtOtherAction rOth)
            {
                return Consume(t.Actor, rOth.Item)
                    && MayTarget(t.Actor, rOth.Victim)
                    && ItemThrown.Handle(new(t.Actor, rOth.Victim, rOth.Victim.Position(), rOth.Item))
                    && HandleRangedAttack(t.Actor, rOth.Victim, ref cost, rOth.Item);
            }
            else if (action is ThrowItemAtPointAction rDir)
            {
                Actor victim;
                // the point is relative to the actor's position
                var newPos = t.Actor.Position() + rDir.Point;
                switch (rDir.Item.ThrowableProperties.Throw)
                {
                    case ThrowName.Arc:
                        TryFindVictim(newPos, t.Actor, out victim);
                        break;
                    case ThrowName.Line:
                        victim = Shapes.Line(t.Actor.Position(), newPos)
                            .Skip(1)
                            .TakeWhile(x => _floorSystem.GetCellAt(t.Actor.FloorId(), x)?.IsWalkable(t.Actor) ?? false)
                            .TrySelect(p => (TryFindVictim(p, t.Actor, out var victim), victim))
                            .LastOrDefault();
                        break;
                    default: throw new NotSupportedException();
                }
                if (victim != null)
                {
                    return Consume(t.Actor, rDir.Item)
                        && ItemThrown.Handle(new(t.Actor, victim, newPos, rDir.Item))
                        && HandleRangedAttack(t.Actor, victim, ref cost, rDir.Item);
                }
                else
                {
                    var lastNonWall = Shapes.Line(t.Actor.Position(), newPos)
                        .TakeWhile(x => _floorSystem.GetCellAt(t.Actor.FloorId(), x)?.IsWalkable(t.Actor) ?? false)
                        .Last();
                    return Consume(t.Actor, rDir.Item)
                        && ItemThrown.Handle(new(t.Actor, null, lastNonWall, rDir.Item));
                }
            }
            else throw new NotSupportedException(action.GetType().Name);

            bool Consume(Actor a, Throwable i)
            {
                if (i.ThrowableProperties.ThrowsUseCharges)
                {
                    return ItemConsumed.Handle(new(a, i));
                }
                // TODO
                return true;
            }

            bool HandleRangedAttack(Actor attacker, Actor victim, ref int? cost, Throwable item)
            {
                var floorId = attacker.FloorId();
                var aPos = attacker.Position();
                var vPos = victim.Position();
                switch (item.ThrowableProperties.Throw)
                {
                    case { } when _floorSystem.IsLineOfSightBlocked(item, aPos, vPos):
                        return false;
                    // If the throwable hits in a straight line, we need to figure out the first actor across that line
                    case ThrowName.Line:
                        victim = Shapes.Line(aPos, vPos)
                            .SelectMany(p => _floorSystem.GetActorsAt(floorId, p))
                            .Except(new[] { attacker })
                            .First();
                        break;
                }
                return HandleAttack(AttackName.Ranged, t.Actor, victim, ref cost, item, out _, out _);
            }

        }
    }
}
