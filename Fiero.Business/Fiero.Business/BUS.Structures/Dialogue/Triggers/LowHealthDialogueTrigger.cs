using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class LowHealthDialogueTrigger<TDialogue> : PlayerInSightDialogueTrigger<TDialogue>
        where TDialogue : struct, Enum
    {
        public float PercentageThreshold { get; set; } = 0.5f;

        public LowHealthDialogueTrigger(GameSystems sys, bool repeatable, params TDialogue[] nodeChoices)
            : base(sys, repeatable, nodeChoices)
        {

        }

        public override bool TryTrigger(FloorId floorId, PhysicalEntity speaker, out IEnumerable<DrawableEntity> listeners)
        {
            listeners = default;
            if(speaker is Actor a) {
                var healthPercentage = (a.ActorProperties.Stats.Health / (float)a.ActorProperties.Stats.MaximumHealth);
                if (healthPercentage <= PercentageThreshold && base.TryTrigger(floorId, speaker, out listeners)) {
                    return listeners.Any();
                }
            }
            return false;
        }
    }
}
