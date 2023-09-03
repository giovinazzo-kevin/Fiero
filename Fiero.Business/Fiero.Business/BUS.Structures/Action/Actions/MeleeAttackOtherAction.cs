namespace Fiero.Business
{
    public readonly struct MeleeAttackOtherAction : IAction
    {
        public readonly Actor Victim;
        public readonly Weapon[] Weapons;
        public MeleeAttackOtherAction(Actor victim, Weapon[] weapons)
        {
            Victim = victim;
            Weapons = weapons;
        }
        ActionName IAction.Name => ActionName.MeleeAttack;
        int? IAction.Cost => 100;
    }
}
