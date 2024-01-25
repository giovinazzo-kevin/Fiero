namespace Fiero.Business
{
    public readonly struct ThrowItemAtOtherAction(Actor victim, Projectile item) : IAction
    {
        public readonly Actor Victim = victim;
        public readonly Projectile Item = item;

        ActionName IAction.Name => ActionName.Throw;
        int? IAction.Cost => 100;
    }
}
