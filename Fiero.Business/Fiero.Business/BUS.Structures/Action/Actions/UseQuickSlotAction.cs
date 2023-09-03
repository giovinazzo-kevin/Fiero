namespace Fiero.Business
{
    public readonly struct UseQuickSlotAction : IAction
    {
        public readonly int Slot;
        public readonly IAction Action;
        public readonly QuickSlotHelper QuickSlotHelper;
        public UseQuickSlotAction(QuickSlotHelper helper, int slot, IAction action)
        {
            QuickSlotHelper = helper;
            Slot = slot;
            Action = action;
        }
        ActionName IAction.Name => ActionName.Macro;
        int? IAction.Cost => 0;
    }
}
