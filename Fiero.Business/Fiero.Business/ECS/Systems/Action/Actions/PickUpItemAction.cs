using Fiero.Core;

namespace Fiero.Business
{
    public readonly struct PickUpItemAction : IAction
    {
        public readonly Item Item;
        public PickUpItemAction(Item item)
        {
            Item = item;
        }
        ActionName IAction.Name => ActionName.Interact;
        int? IAction.Cost => 1;
    }
}
