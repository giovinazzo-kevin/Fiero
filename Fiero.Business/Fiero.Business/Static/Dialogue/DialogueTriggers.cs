using System.Collections.Generic;

namespace Fiero.Business
{
    public static class DialogueTriggers
    {
        public static void Set(NpcName type, DialogueComponent component)
        {
            switch (type) {
                case NpcName.GreatKingRat:
                    foreach (var t in GreatKingRat()) component.Triggers.Add(t);
                    break;
            }
            static IEnumerable<IDialogueTrigger> GreatKingRat()
            {
                yield return new PlayerInSightDialogueTrigger<GKRDialogueName>(
                    GKRDialogueName.JustMet, repeatable: false);
            }
        }

        public static void Set(FeatureName type, DialogueComponent component)
        {
            switch (type) {
                case FeatureName.Shrine:
                    foreach (var t in Shrine()) component.Triggers.Add(t);
                    break;
            }
            static IEnumerable<IDialogueTrigger> Shrine()
            {
                yield return new BumpedByPlayerDialogueTrigger<ShrineDialogueName>(
                    ShrineDialogueName.Smintheus, repeatable: true);
            }
        }
    }
}
