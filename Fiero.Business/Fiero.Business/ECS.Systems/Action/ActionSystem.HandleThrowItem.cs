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
                switch (rDir.Item.ProjectileProperties.Trajectory)
                {
                    case TrajectoryName.Arc:
                        TryFindVictim(newPos, t.Actor, out victim);
                        break;
                    case TrajectoryName.Line:
                        victim = Shapes.Line(t.Actor.Position(), newPos)
                            .Skip(1)
                            .TakeWhile(x => !_floorSystem.GetCellAt(t.Actor.FloorId(), x)?.BlocksMovement() ?? false)
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
                        .TakeWhile(x => !_floorSystem.GetCellAt(t.Actor.FloorId(), x)?.BlocksMovement() ?? false)
                        .Last();
                    return Consume(t.Actor, rDir.Item)
                        && ItemThrown.Handle(new(t.Actor, null, lastNonWall, rDir.Item));
                }
            }
            else throw new NotSupportedException(action.GetType().Name);

            bool Consume(Actor a, Projectile i)
            {
                if (i.ProjectileProperties.ThrowsUseCharges)
                {
                    return ItemConsumed.Handle(new(a, i));
                }
                // TODO
                return true;
            }
        }
    }
}
