namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ActorHealedEvent(Entity Source, Actor Target, Entity Means, int Heal);
    }
}
