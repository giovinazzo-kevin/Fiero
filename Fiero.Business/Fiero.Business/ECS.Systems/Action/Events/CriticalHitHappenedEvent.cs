namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct CriticalHitHappenedEvent(Actor Attacker, Actor[] Victims, Entity Weapon, int Damage);
    }
}
