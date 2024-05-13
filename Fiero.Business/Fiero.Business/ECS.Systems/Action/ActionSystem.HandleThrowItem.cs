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
                int? shootCost = null;
                // the point is relative to the actor's position
                var newPos = t.Actor.Position() + rDir.Point;
                if (!Consume(t.Actor, rDir.Item))
                    return false;
                switch (rDir.Item.ProjectileProperties.Trajectory)
                {
                    case TrajectoryName.Arc:
                        newPos = Shapes.Line(t.Actor.Position(), newPos)
                            .Skip(1)
                            .TakeWhile(x => !_floorSystem.GetCellAt(t.Actor.FloorId(), x)?.BlocksMovement(excludeFlat: true, excludeFeatures: true) ?? false)
                            .DefaultIfEmpty(newPos)
                            .LastOrDefault();
                        TryFindVictim(newPos, t.Actor, out victim);
                        if (!HandleVictim(out shootCost))
                            return false;
                        return true;
                    case TrajectoryName.Line:
                        var newPosOptions = Shapes.Line(t.Actor.Position(), newPos)
                            .Skip(1)
                            .TakeWhile(x => !_floorSystem.GetCellAt(t.Actor.FloorId(), x)?.BlocksMovement(excludeFlat: true) ?? false)
                            .DefaultIfEmpty(newPos);
                        foreach (var p in newPosOptions)
                        {
                            newPos = p;
                            if (TryFindVictim(p, t.Actor, out victim))
                            {
                                if (!HandleVictim(out shootCost))
                                    return false;
                                if (!rDir.Item.ProjectileProperties.Piercing)
                                {
                                    cost += shootCost;
                                    return true;
                                }
                            }
                        }
                        cost += shootCost;
                        return ItemThrown.Handle(new(t.Actor, null, newPos, rDir.Item));
                    default: throw new NotSupportedException();
                }
                bool HandleVictim(out int? cost)
                {
                    cost = 0;
                    if (victim != null)
                    {
                        return ItemThrown.Handle(new(t.Actor, victim, newPos, rDir.Item))
                            && HandleRangedAttack(t.Actor, victim, ref cost, rDir.Item);
                    }
                    return false;
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
