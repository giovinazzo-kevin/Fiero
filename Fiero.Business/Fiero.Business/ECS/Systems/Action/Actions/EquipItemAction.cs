namespace Fiero.Business
{
    public readonly struct EquipItemAction : IAction
    {
        public readonly Item Item;
        public EquipItemAction(Item item)
        {
            Item = item;
        }
        ActionName IAction.Name => ActionName.Organize;
        int? IAction.Cost => 100;
    }
}
