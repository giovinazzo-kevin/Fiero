namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ActorLeveledUpEvent(Actor Actor, int OldLevel, int NewLevel);
    }
}
