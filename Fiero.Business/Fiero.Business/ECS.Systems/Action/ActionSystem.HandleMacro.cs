using Fiero.Core;
using System;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleMacro(ActorTime t, ref IAction action, ref int? cost)
        {
            if (action is UseQuickSlotAction slot)
            {
                action = slot.Action;
                cost += HandleAction(t, ref action);
                slot.QuickSlotHelper.OnActionResolved(slot.Slot);
                return true;
            }
            else throw new NotSupportedException();
        }
    }
}
