namespace Fiero.Business
{
    public readonly struct ZapWandAtOtherAction : IAction
    {
        public readonly Wand Wand;
        public readonly Actor Victim;

        public ZapWandAtOtherAction(Wand wand, Actor actor)
        {
            Wand = wand;
            Victim = actor;
        }

        ActionName IAction.Name => ActionName.Zap;
        int? IAction.Cost => 100;
    }
}
