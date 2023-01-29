namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ActorIntentEvent(Actor Actor, IAction Intent, int TurnId, int Time);
    }
}
