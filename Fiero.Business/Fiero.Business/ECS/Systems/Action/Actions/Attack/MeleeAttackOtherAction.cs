namespace Fiero.Business
{
    public readonly struct MeleeAttackOtherAction : IAction
    {
        public readonly Actor Victim;
        public MeleeAttackOtherAction(Actor victim)
        {
            Victim = victim;
        }
        ActionName IAction.Name => ActionName.Attack;
        int? IAction.Cost => 100;
    }
}
