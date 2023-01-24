namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ActorSpawnedEvent(Actor Actor);
    }
}
