using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Fiero.Business
{
    public class BumpedByPlayerDialogueTrigger<TDialogue> : PlayerInSightDialogueTrigger<TDialogue>
        where TDialogue : struct, Enum
    {
        public BumpedByPlayerDialogueTrigger(TDialogue node, bool repeatable)
            : base(node, repeatable)
        {

        }

        public override bool TryTrigger(Floor floor, Drawable speaker, out IEnumerable<Drawable> listeners)
        {
            if(base.TryTrigger(floor, speaker, out listeners)) {
                listeners = listeners
                    .Where(l => l is Actor a && a.Action.Direction == speaker.Physics.Position - l.Physics.Position);
                return listeners.Any();
            }
            return false;
        }
    }
}
