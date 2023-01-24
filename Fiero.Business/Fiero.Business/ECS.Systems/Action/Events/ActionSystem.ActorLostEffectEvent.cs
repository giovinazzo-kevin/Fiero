namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ActorLostEffectEvent(Actor Actor, Effect Effect);
    }
}
