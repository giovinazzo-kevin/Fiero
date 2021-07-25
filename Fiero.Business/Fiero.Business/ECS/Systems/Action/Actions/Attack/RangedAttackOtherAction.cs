namespace Fiero.Business
{
    public readonly struct RangedAttackOtherAction : IAction
    {
        public readonly Actor Victim;
        public readonly Weapon[] Weapons;
        public RangedAttackOtherAction(Actor victim, params Weapon[] weapons)
        {
            Victim = victim;
            Weapons = weapons;
        }
        ActionName IAction.Name => ActionName.Attack;
        int? IAction.Cost => 100;
    }
}
