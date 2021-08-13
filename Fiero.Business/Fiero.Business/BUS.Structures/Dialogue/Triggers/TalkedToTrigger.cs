﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class TalkedToTrigger<TDialogue> : PlayerInSightDialogueTrigger<TDialogue>
        where TDialogue : struct, Enum
    {
        public TalkedToTrigger(GameSystems sys, bool repeatable, params TDialogue[] nodeChoices)
            : base(sys, repeatable, nodeChoices)
        {

        }

        public override bool TryTrigger(FloorId floorId, PhysicalEntity speaker, out IEnumerable<DrawableEntity> listeners)
        {
            if (base.TryTrigger(floorId, speaker, out listeners)) {
                listeners = listeners
                    .Where(l => l is Actor a && a.Action.LastAction is InitiateConversationAction i && i.NPC.Id == speaker.Id);
                return listeners.Any();
            }
            return false;
        }
    }
}