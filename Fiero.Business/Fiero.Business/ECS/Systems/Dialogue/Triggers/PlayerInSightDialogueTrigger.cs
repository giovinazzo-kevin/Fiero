using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class PlayerInSightDialogueTrigger<TDialogue> : DialogueTrigger<TDialogue>
        where TDialogue : struct, Enum
    {
        public float DistanceThreshold { get; set; } = 5;

        public PlayerInSightDialogueTrigger(TDialogue node, bool repeatable) 
            : base(node, repeatable)
        {

        }

        public override bool TryTrigger(Floor floor, Drawable speaker, out IEnumerable<Drawable> listeners)
        {
            listeners = floor.Actors
                .Where(a => a.ActorProperties.Type == ActorName.Player 
                    && a.DistanceFrom(speaker) < DistanceThreshold
                    && a.CanSee(speaker));
            return listeners.Any();
        }
    }
}
