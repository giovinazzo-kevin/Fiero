namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorAttackedEvent
        {
            public readonly AttackName Type;
            public readonly Actor Attacker;
            public readonly Actor Victim;
            public readonly Entity Weapon;
            public readonly int Damage;
            public readonly int Delay;
            public ActorAttackedEvent(AttackName type, Actor attacker, Actor victim, Entity weapon, int damage, int delay)
                => (Type, Attacker, Victim, Weapon, Damage, Delay) = (type, attacker, victim, weapon, damage, delay);
        }
    }
}
