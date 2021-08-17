using Fiero.Core;
using SFML.Graphics;
using SFML.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using Unconcern.Common;

namespace Fiero.Business
{
    public partial class ActionSystem : EcsSystem
    {
        private bool HandleWait(ActorTime t, ref IAction action, ref int? cost)
        {
            if (!(action is WaitAction))
                throw new NotSupportedException();
            return ActorHealed.Handle(new(t.Actor, t.Actor, t.Actor, 1))
                && ActorWaited.Handle(new(t.Actor, CurrentTurn, t.Time));
        }
    }
}
