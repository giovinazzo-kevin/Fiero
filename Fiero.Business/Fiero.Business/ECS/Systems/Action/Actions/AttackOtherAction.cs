namespace Fiero.Business
{
    public readonly struct AttackOtherAction : IAction
    {
        public readonly Actor Victim;
        public AttackOtherAction(Actor victim)
        {
            Victim = victim;
        }
        ActionName IAction.Name => ActionName.Attack;
        int? IAction.Cost => 10;
    }
}
