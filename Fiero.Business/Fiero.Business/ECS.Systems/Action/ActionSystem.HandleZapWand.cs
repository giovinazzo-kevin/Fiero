namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleZapWand(ActorTime t, ref IAction action, ref int? cost)
        {
            if (action is ZapWandAtOtherAction rOth)
            {
                return ItemConsumed.Handle(new(t.Actor, rOth.Wand))
                    && HandleMagicAttack(t.Actor, rOth.Victim, ref cost, rOth.Wand);
            }
            else if (action is ZapWandAtPointAction rDir)
            {
                // the point is relative to the actor's position
                var newPos = t.Actor.Position() + rDir.Point;
                var victim = Shapes.Line(t.Actor.Position(), newPos)
                    .Skip(1)
                    .TakeWhile(x => _floorSystem.GetCellAt(t.Actor.FloorId(), x)?.IsWalkable(t.Actor) ?? false)
                    .TrySelect(p => (TryFindVictim(p, t.Actor, out var victim), victim))
                    .LastOrDefault();
                if (victim is { })
                {
                    return ItemConsumed.Handle(new(t.Actor, rDir.Wand))
                        && HandleMagicAttack(t.Actor, victim, ref cost, rDir.Wand);
                }
                else
                {
                    var lastNonWall = Shapes.Line(t.Actor.Position(), newPos)
                        .TakeWhile(x => _floorSystem.GetCellAt(t.Actor.FloorId(), x)?.IsWalkable(t.Actor) ?? false)
                        .Last();
                    return ItemConsumed.Handle(new(t.Actor, rDir.Wand))
                        && WandZapped.Handle(new(t.Actor, null, lastNonWall, rDir.Wand));
                }
            }
            else throw new NotSupportedException(action.GetType().Name);

            bool HandleMagicAttack(Actor attacker, Actor victim, ref int? cost, Wand item)
            {
                var vPos = victim.Position();
                return WandZapped.Handle(new(attacker, victim, vPos, item))
                    && HandleAttack(AttackName.Magic, t.Actor, victim, ref cost, new[] { item }, out _, out _);
            }
        }
    }
}
