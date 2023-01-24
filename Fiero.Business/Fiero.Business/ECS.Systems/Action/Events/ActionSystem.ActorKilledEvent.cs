namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ActorKilledEvent(Actor Killer, Actor Victim);
    }
}
