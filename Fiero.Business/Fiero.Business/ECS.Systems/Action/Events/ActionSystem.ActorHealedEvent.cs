namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorHealedEvent
        {
            public readonly Entity Source;
            public readonly Actor Target;
            public readonly Entity Means;
            public readonly int Heal;

            public ActorHealedEvent(Entity source, Actor victim, Entity implement, int heal)
                => (Source, Target, Means, Heal) = (source, victim, implement, heal);
        }
    }
}
