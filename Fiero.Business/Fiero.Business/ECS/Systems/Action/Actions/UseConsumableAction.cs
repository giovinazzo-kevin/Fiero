namespace Fiero.Business
{
    public readonly struct UseConsumableAction : IAction
    {
        public readonly Consumable Item;
        public UseConsumableAction(Consumable item)
        {
            Item = item;
        }
        ActionName IAction.Name => ActionName.Organize;
        int? IAction.Cost => 1;
    }
}
