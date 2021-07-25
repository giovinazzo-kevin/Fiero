namespace Fiero.Business
{
    public readonly struct MeleeAttackOtherAction : IAction
    {
        public readonly Actor Victim;
        public readonly Weapon[] Weapons;
        public MeleeAttackOtherAction(Actor victim, params Weapon[] weapons)
        {
            Victim = victim;
            Weapons = weapons;
        }
        ActionName IAction.Name => ActionName.Attack;
        int? IAction.Cost => 100;
    }
}
