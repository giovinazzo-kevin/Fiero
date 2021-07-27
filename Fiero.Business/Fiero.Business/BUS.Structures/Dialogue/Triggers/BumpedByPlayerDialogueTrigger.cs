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
        public BumpedByPlayerDialogueTrigger(GameSystems sys, TDialogue node, bool repeatable)
            : base(sys, node, repeatable)
        {

        }

        public override bool TryTrigger(FloorId floorId, PhysicalEntity speaker, out IEnumerable<DrawableEntity> listeners)
        {
            if(base.TryTrigger(floorId, speaker, out listeners)) {
                listeners = listeners
                    .Where(l => l is Actor a && a.Action.LastAction is InteractWithFeatureAction i && i.Feature.Id == speaker.Id);
                return listeners.Any();
            }
            return false;
        }
    }
}
