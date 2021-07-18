namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorAttackedEvent
        {
            public readonly Actor Attacker;
            public readonly Actor Victim;
            public ActorAttackedEvent(Actor attacker, Actor victim)
                => (Attacker, Victim) = (attacker, victim);
        }
    }
}
