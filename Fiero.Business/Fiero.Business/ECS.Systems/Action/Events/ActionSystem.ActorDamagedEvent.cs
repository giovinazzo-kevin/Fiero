namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorDamagedEvent
        {
            public readonly Entity Source;
            public readonly Weapon[] Weapons;

            public readonly Actor Victim;
            public readonly int Damage;
            public ActorDamagedEvent(Entity source, Weapon[] weapons, Actor victim, int damage)
                => (Source, Weapons, Victim, Damage) = (source, weapons, victim, damage);
        }
    }
}
