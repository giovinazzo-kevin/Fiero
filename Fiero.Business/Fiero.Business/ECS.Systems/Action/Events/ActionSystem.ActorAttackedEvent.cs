using Ergo.Lang;

namespace Fiero.Business
{
    public partial class ActionSystem
    {
        [Term(Marshalling = TermMarshalling.Named)]
        public readonly record struct ActorAttackedEvent(AttackName Type, Actor Attacker, Actor[] Victims, Entity[] Weapons, int Damage, int Delay);
    }
}
