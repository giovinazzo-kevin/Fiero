namespace Fiero.Business
{
    public readonly struct RangedAttackOtherAction : IAction
    {
        public readonly Actor Victim;
        public RangedAttackOtherAction(Actor victim)
        {
            Victim = victim;
        }
        ActionName IAction.Name => ActionName.Attack;
        int? IAction.Cost => 100;
    }
}
