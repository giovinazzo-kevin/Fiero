namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct EntityDespawnedEvent(Entity Entity);
    }
}
