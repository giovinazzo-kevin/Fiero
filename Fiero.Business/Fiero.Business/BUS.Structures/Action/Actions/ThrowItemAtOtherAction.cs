namespace Fiero.Business
{
    public readonly struct ThrowItemAtOtherAction : IAction
    {
        public readonly Actor Victim;
        public readonly Throwable Item;
        public ThrowItemAtOtherAction(Actor victim, Throwable item)
        {
            Victim = victim;
            Item = item;
        }
        ActionName IAction.Name => ActionName.Throw;
        int? IAction.Cost => 100;
    }
}
