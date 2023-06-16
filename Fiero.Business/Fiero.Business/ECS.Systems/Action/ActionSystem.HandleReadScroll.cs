using Fiero.Core;
using System;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleReadScroll(ActorTime t, ref IAction action, ref int? cost)
        {
            if (action is ReadScrollAction a)
            {
                return ItemConsumed.Handle(new(t.Actor, a.Scroll))
                    && ScrollRead.Handle(new(t.Actor, a.Scroll));
            }
            else throw new NotSupportedException(action.GetType().Name);
        }
    }
}
