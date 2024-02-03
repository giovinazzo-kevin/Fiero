namespace Fiero.Business
{
    public class ManualDialogueTrigger : DialogueTrigger
    {
        public ManualDialogueTrigger(MetaSystem sys, params string[] nodeChoices)
            : base(sys, false, nodeChoices)
        {

        }

        public override bool TryTrigger(FloorId floorId, PhysicalEntity speaker, out IEnumerable<DrawableEntity> listeners)
        {
            listeners = Systems.Get<DungeonSystem>().GetAllActors(floorId)
                .Where(a => a.CanHear(speaker));
            return listeners.Any();
        }
    }
}
