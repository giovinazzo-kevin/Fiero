namespace Fiero.Business
{
    public readonly struct MeleeAttackOtherAction : IAction
    {
        public readonly Actor Victim;
        public readonly Weapon Weapon;
        public MeleeAttackOtherAction(Actor victim, Weapon weapon)
        {
            Victim = victim;
            Weapon = weapon;
        }
        ActionName IAction.Name => ActionName.MeleeAttack;
        int? IAction.Cost => 100;
    }
}
