namespace Fiero.Business
{
    public readonly struct ReadScrollAction : IAction
    {
        public readonly Scroll Scroll;

        public ReadScrollAction(Scroll scroll)
        {
            Scroll = scroll;
        }

        ActionName IAction.Name => ActionName.Read;
        int? IAction.Cost => 100;
    }
}
