namespace Fiero.Business
{
    public readonly struct EquipOrUnequipItemAction : IAction
    {
        public readonly Item Item;
        public EquipOrUnequipItemAction(Item item)
        {
            Item = item;
        }
        ActionName IAction.Name => ActionName.Organize;
        int? IAction.Cost => 0;
    }
}
