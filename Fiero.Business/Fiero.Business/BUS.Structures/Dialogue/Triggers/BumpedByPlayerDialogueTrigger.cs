namespace Fiero.Business
{
    public class BumpedByPlayerDialogueTrigger : PlayerInSightDialogueTrigger
    {
        public BumpedByPlayerDialogueTrigger(MetaSystem sys, bool repeatable, params string[] nodeChoices)
            : base(sys, repeatable, nodeChoices)
        {

        }

        public override bool TryTrigger(FloorId floorId, PhysicalEntity speaker, out IEnumerable<DrawableEntity> listeners)
        {
            if (base.TryTrigger(floorId, speaker, out listeners))
            {
                listeners = listeners
                    .Where(l => l is Actor a && a.Action.LastAction is InteractWithFeatureAction i && i.Feature.Id == speaker.Id);
                return listeners.Any();
            }
            return false;
        }
    }
}
