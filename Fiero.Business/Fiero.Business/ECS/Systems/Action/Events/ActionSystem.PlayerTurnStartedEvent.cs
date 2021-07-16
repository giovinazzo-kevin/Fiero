namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorTurnEvent
        {
            public readonly Actor Actor;
            public readonly int TurnId;
            public ActorTurnEvent(Actor actor, int turnId)
                => (Actor, TurnId) = (actor, turnId);
        }
    }
}
