using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class LowHealthDialogueTrigger<TDialogue> : PlayerInSightDialogueTrigger<TDialogue>
        where TDialogue : struct, Enum
    {
        public float PercentageThreshold { get; set; } = 0.5f;

        public LowHealthDialogueTrigger(TDialogue node, bool repeatable)
            : base(node, repeatable)
        {

        }

        public override bool TryTrigger(Floor floor, Drawable speaker, out IEnumerable<Drawable> listeners)
        {
            listeners = default;
            if(speaker is Actor a) {
                var healthPercentage = (a.Properties.Health / (float)a.Properties.MaximumHealth);
                if (healthPercentage <= PercentageThreshold && base.TryTrigger(floor, speaker, out listeners)) {
                    return listeners.Any();
                }
            }
            return false;
        }
    }
}
