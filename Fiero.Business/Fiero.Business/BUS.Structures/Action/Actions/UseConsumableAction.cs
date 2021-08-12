namespace Fiero.Business
{
    public readonly struct UseConsumableAction : IAction
    {
        private readonly ActionName Name;
        public readonly Consumable Item;


        public UseConsumableAction(Consumable item)
        {
            Item = item;
            if(Item.TryCast<Potion>(out _)) {
                Name = ActionName.Drink;
            }
            else if (Item.TryCast<Scroll>(out _)) {
                Name = ActionName.Read;
            }
            else if (Item.TryCast<Wand>(out _)) {
                Name = ActionName.Zap;
            }
            else {
                Name = ActionName.Organize;
            }
        }
        ActionName IAction.Name => Name;
        int? IAction.Cost => 100;
    }
}
