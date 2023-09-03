namespace Fiero.Business
{
    public readonly struct EquipOrUnequipItemAction : IAction
    {
        public readonly Equipment Item;
        public EquipOrUnequipItemAction(Equipment item)
        {
            Item = item;
        }
        ActionName IAction.Name => ActionName.Organize;
        int? IAction.Cost => 0;
    }
}
