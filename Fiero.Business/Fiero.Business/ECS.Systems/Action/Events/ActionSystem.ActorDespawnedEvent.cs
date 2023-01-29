namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ActorDespawnedEvent(Actor Actor);
    }
}
