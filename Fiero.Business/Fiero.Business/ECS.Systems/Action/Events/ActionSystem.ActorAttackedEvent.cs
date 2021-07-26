namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorAttackedEvent
        {
            public readonly AttackName Type;
            public readonly Actor Attacker;
            public readonly Actor Victim;
            public readonly Weapon[] Weapons;
            public ActorAttackedEvent(AttackName type, Actor attacker, Actor victim, Weapon[] weapons)
                => (Type, Attacker, Victim, Weapons) = (type, attacker, victim, weapons);
        }

        public class ActorAttackedEventResult : EventResult
        {
            public readonly int Damage;
            public readonly int SwingDelay;

            public ActorAttackedEventResult(int damage, int swingDelay, bool result) : base(result)
            {
                Damage = damage;
                SwingDelay = swingDelay;
            }
        }
    }
}
