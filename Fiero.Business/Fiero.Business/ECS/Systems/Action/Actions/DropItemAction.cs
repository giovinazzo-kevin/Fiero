namespace Fiero.Business
{
    public readonly struct DropItemAction : IAction
    {
        public readonly Item Item;
        public DropItemAction(Item item)
        {
            Item = item;
        }
        ActionName IAction.Name => ActionName.Organize;
        int? IAction.Cost => 1;
    }
}
