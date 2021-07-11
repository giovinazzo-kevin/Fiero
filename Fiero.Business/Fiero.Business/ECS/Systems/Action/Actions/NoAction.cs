namespace Fiero.Business
{
    public readonly struct NoAction : IAction
    {
        ActionName IAction.Name => ActionName.None;
    }
}
