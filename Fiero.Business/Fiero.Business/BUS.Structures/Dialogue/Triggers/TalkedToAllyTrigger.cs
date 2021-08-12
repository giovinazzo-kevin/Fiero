using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class TalkedToAllyTrigger<TDialogue> : TalkedToTrigger<TDialogue>
        where TDialogue : struct, Enum
    {
        public TalkedToAllyTrigger(GameSystems sys, bool repeatable, params TDialogue[] nodeChoices) 
            : base(sys, repeatable, nodeChoices)
        {

        }

        public override bool TryTrigger(FloorId floorId, PhysicalEntity speaker, out IEnumerable<DrawableEntity> listeners)
        {
            listeners = Enumerable.Empty<DrawableEntity>();
            if (!(speaker is Actor s)) {
                return false;
            }

            if (base.TryTrigger(floorId, speaker, out listeners)) {
                listeners = listeners
                    .Where(l => l is Actor a && Systems.Faction.GetRelationships(s, a).Left.IsFriendly());
                return listeners.Any();
            }
            return false;
        }
    }
}
