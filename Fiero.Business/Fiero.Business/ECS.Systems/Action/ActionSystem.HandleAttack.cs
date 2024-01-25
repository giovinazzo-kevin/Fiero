namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleMeleeAttack(ActorTime t, ref IAction action, ref int? cost)
        {
            var victim = default(Actor);
            if (action is MeleeAttackOtherAction oth)
            {
                victim = oth.Victim;
                return HandleMeleeAttack(ref cost, oth.Weapons);
            }
            else if (action is MeleeAttackPointAction dir)
            {
                var newPos = t.Actor.Position() + dir.Point;
                return TryFindVictim(newPos, t.Actor, out victim)
                    && HandleMeleeAttack(ref cost, dir.Weapons);
            }
            else throw new NotSupportedException(action.GetType().Name);

            bool HandleMeleeAttack(ref int? cost, Weapon[] weapons)
            {
                if (t.Actor.DistanceFrom(victim) >= 2)
                {
                    // out of reach
                    return false;
                }
                return HandleAttack(AttackName.Melee, t.Actor, [victim], ref cost, weapons, out _, out _);
            }
        }
    }
}
