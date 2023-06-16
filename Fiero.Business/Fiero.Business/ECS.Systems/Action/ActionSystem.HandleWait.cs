using Fiero.Core;
using System;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleWait(ActorTime t, ref IAction action, ref int? cost)
        {
            if (!(action is WaitAction))
                throw new NotSupportedException();
            return ActorWaited.Handle(new(t.Actor, CurrentTurn, t.Time));
        }
    }
}
