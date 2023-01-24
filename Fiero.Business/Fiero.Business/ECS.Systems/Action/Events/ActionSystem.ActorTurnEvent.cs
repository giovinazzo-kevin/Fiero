namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ActorTurnEvent(Actor Actor, int TurnId, int Time);
    }
}
