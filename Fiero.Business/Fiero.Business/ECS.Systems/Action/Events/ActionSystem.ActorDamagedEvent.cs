namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ActorDamagedEvent(Entity Source, Actor Victim, Entity Weapon, int Damage);
    }
}
