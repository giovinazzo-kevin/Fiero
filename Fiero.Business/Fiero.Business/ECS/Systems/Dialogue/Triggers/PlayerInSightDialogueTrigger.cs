using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class PlayerInSightDialogueTrigger<TDialogue> : DialogueTrigger<TDialogue>
        where TDialogue : struct, Enum
    {
        public float DistanceThreshold { get; set; } = 5;

        public PlayerInSightDialogueTrigger(GameSystems sys, TDialogue node, bool repeatable) 
            : base(sys, node, repeatable)
        {

        }

        public override bool TryTrigger(FloorId floorId, Drawable speaker, out IEnumerable<Drawable> listeners)
        {
            listeners = Systems.Floor.GetAllActors(floorId)
                .Where(a => a.ActorProperties.Type == ActorName.Player 
                    && a.DistanceFrom(speaker) < DistanceThreshold
                    && a.CanSee(speaker));
            return listeners.Any();
        }
    }
}
