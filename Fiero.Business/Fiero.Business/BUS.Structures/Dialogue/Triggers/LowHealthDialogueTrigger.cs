namespace Fiero.Business
{
    public class LowHealthDialogueTrigger : PlayerInSightDialogueTrigger
    {
        public float PercentageThreshold { get; set; } = 0.5f;

        public LowHealthDialogueTrigger(MetaSystem sys, bool repeatable, params string[] nodeChoices)
            : base(sys, repeatable, nodeChoices)
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
