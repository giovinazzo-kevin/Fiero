namespace Fiero.Business
{
    public class TalkedToHostileTrigger : TalkedToTrigger
    {
        public TalkedToHostileTrigger(MetaSystem sys, bool repeatable, params string[] nodeChoices)
            : base(sys, repeatable, nodeChoices)
        {

        }

        public override bool TryTrigger(FloorId floorId, PhysicalEntity speaker, out IEnumerable<DrawableEntity> listeners)
        {
            listeners = Enumerable.Empty<DrawableEntity>();
            if (!(speaker is Actor s))
            {
                return false;
            }

            if (base.TryTrigger(floorId, speaker, out listeners))
            {
                listeners = listeners
                    .Where(l => l is Actor a && Systems.Get<FactionSystem>().GetRelations(s, a).Left.IsHostile());
                return listeners.Any();
            }
            return false;
        }
    }
}
