namespace Fiero.Business
{
    public class PlayerInSightDialogueTrigger : DialogueTrigger
    {
        public float DistanceThreshold { get; set; } = 5;

        public PlayerInSightDialogueTrigger(MetaSystem sys, bool repeatable, params string[] nodeChoices)
            : base(sys, repeatable, nodeChoices)
        {

        }

        public override bool TryTrigger(FloorId floorId, PhysicalEntity speaker, out IEnumerable<DrawableEntity> listeners)
        {
            listeners = Systems.Get<DungeonSystem>().GetAllActors(floorId)
                .Where(a => a.IsPlayer()
                    && a.DistanceFrom(speaker) < DistanceThreshold
                    && a.CanSee(speaker));
            return listeners.Any();
        }
    }
}
