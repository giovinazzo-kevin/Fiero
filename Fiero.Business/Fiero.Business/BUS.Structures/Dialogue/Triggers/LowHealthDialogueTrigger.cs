namespace Fiero.Business
{
    public class LowHealthDialogueTrigger<TDialogue> : PlayerInSightDialogueTrigger<TDialogue>
        where TDialogue : struct, Enum
    {
        public float PercentageThreshold { get; set; } = 0.5f;

        public LowHealthDialogueTrigger(MetaSystem sys, bool repeatable, string path, params TDialogue[] nodeChoices)
            : base(sys, repeatable, path, nodeChoices)
        {

        }

        public override bool TryTrigger(FloorId floorId, PhysicalEntity speaker, out IEnumerable<DrawableEntity> listeners)
        {
            listeners = default;
            if (speaker is Actor a)
            {
                if (a.ActorProperties.Health.Percentage <= PercentageThreshold && base.TryTrigger(floorId, speaker, out listeners))
                {
                    return listeners.Any();
                }
            }
            return false;
        }
    }
}
