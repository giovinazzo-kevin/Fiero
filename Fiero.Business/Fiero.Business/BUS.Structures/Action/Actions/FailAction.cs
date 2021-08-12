namespace Fiero.Business
{
    public readonly struct FailAction : IAction
    {
        ActionName IAction.Name => ActionName.Fail;
        int? IAction.Cost => null;
    }
}
