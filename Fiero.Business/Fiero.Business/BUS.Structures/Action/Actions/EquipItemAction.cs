namespace Fiero.Business
{
    public readonly struct EquipItemAction : IAction
    {
        public readonly Equipment Item;
        public EquipItemAction(Equipment item)
        {
            Item = item;
        }
        ActionName IAction.Name => ActionName.Organize;
        int? IAction.Cost => 0;
    }
}
