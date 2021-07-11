namespace Fiero.Business
{
    public readonly struct WaitAction : IAction
    {
        ActionName IAction.Name => ActionName.Wait;
    }
}
