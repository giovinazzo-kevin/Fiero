namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ActorAttackedEvent(AttackName Type, Actor Attacker, Actor Victim, Entity[] Weapons, int Damage, int Delay);
    }
}
