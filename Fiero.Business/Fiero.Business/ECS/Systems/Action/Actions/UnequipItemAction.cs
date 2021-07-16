namespace Fiero.Business
{
    public readonly struct UnequipItemAction : IAction
    {
        public readonly Item Item;
        public UnequipItemAction(Item item)
        {
            Item = item;
        }
        ActionName IAction.Name => ActionName.Organize;
        int? IAction.Cost => 5;
    }
}
