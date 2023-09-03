namespace Fiero.Business
{
    public readonly struct UnequipItemAction : IAction
    {
        public readonly Equipment Item;
        public UnequipItemAction(Equipment item)
        {
            Item = item;
        }
        ActionName IAction.Name => ActionName.Organize;
        int? IAction.Cost => 0;
    }
}
