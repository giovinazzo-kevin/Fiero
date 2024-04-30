namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleShootLauncher(ActorTime t, ref IAction action, ref int? cost)
        {
            if (action is ShootLauncherAtOtherAction rOth)
            {
                var proj = (Projectile)rOth.Launcher.LauncherProperties.Projectile.Clone();
                return Consume(t.Actor, rOth.Launcher)
                    && MayTarget(t.Actor, rOth.Victim)
                    && LauncherShot.Handle(new(t.Actor, rOth.Victim, rOth.Victim.Position(), rOth.Launcher))
                    && HandleRangedAttack(t.Actor, rOth.Victim, ref cost, proj);
            }
            else if (action is ShootLauncherAtPointAction rDir)
            {
                var proj = (Projectile)rDir.Launcher.LauncherProperties.Projectile.Clone();
                Actor victim;
                // the point is relative to the actor's position
                var newPos = t.Actor.Position() + rDir.Point;
                switch (proj.ProjectileProperties.Trajectory)
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
                    return Consume(t.Actor, rDir.Launcher)
                        && LauncherShot.Handle(new(t.Actor, victim, newPos, rDir.Launcher))
                        && HandleRangedAttack(t.Actor, victim, ref cost, proj);
                }
                else
                {
                    var lastNonWall = Shapes.Line(t.Actor.Position(), newPos)
                        .TakeWhile(x => !_floorSystem.GetCellAt(t.Actor.FloorId(), x)?.BlocksMovement() ?? false)
                        .Last();
                    return Consume(t.Actor, rDir.Launcher)
                        && LauncherShot.Handle(new(t.Actor, null, lastNonWall, rDir.Launcher));
                }
            }
            else throw new NotSupportedException(action.GetType().Name);

            bool Consume(Actor a, Launcher l)
            {
                // TODO
                return true;
            }
        }

        bool HandleRangedAttack(Actor attacker, Actor victim, ref int? cost, Projectile item)
        {
            var floorId = attacker.FloorId();
            var aPos = attacker.Position();
            var vPos = victim.Position();
            Actor[] victimAndCollaterals = [victim];
            switch (item.ProjectileProperties.Trajectory)
            {
                case { } when _floorSystem.IsLineOfSightBlocked(attacker, aPos, vPos):
                    return false;
                // If the Projectile hits in a straight line, we need to figure out the first actor across that line
                case TrajectoryName.Line:
                    victimAndCollaterals = Shapes.Line(aPos, vPos)
                        .SelectMany(p => _floorSystem.GetActorsAt(floorId, p))
                        .Except(new[] { attacker })
                        .ToArray();
                    if (!item.ProjectileProperties.Piercing && victimAndCollaterals.Length > 0)
                        victimAndCollaterals = [victimAndCollaterals[0]];
                    break;
            }
            if (!item.ProjectileProperties.Piercing)
                return HandleAttack(AttackName.Ranged, attacker, victimAndCollaterals, ref cost, new[] { item }, out _, out _, out _);
            else
            {
                for (int i = 0; i < victimAndCollaterals.Length; i++)
                {
                    if (!HandleAttack(AttackName.Ranged, attacker, [victimAndCollaterals[i]], ref cost, new[] { item }, out _, out _, out _))
                        return false;
                }
                return true;
            }
        }
    }
}
