namespace Fiero.Business
{
    public readonly struct MoveTowardsAction : IAction
    {
        public readonly Actor Follow;
        public MoveTowardsAction(Actor follow)
        {
            Follow = follow;
        }
        ActionName IAction.Name => ActionName.MeleeAttack;
        int? IAction.Cost => 100;
    }
}
