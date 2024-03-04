namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ActorGainedExperienceEvent(Actor Actor, int ExperienceGained);
    }
}
