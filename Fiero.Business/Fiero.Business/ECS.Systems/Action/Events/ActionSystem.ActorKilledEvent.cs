namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorKilledEvent
        {
            public readonly Actor Killer;
            public readonly Actor Victim;
            public ActorKilledEvent(Actor killer, Actor victim)
                => (Killer, Victim) = (killer, victim);
        }
    }
}
