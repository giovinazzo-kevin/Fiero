namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorDamagedEvent
        {
            public readonly Entity Source;
            public readonly Actor Victim;
            public readonly Entity Weapon;
            public readonly int Damage;

            public ActorDamagedEvent(Entity source, Actor victim, Entity weapon, int damage)
                => (Source, Victim, Weapon, Damage) = (source, victim, weapon, damage);
        }
    }
}
