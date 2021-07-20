namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorAttackedEvent
        {
            public readonly AttackName Type;
            public readonly Actor Attacker;
            public readonly Actor Victim;
            public ActorAttackedEvent(AttackName type, Actor attacker, Actor victim)
                => (Type, Attacker, Victim) = (type, attacker, victim);
        }
    }
}
