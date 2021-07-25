namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorTurnEvent
        {
            public readonly Actor Actor;
            public readonly int TurnId;
            public readonly int Time;
            public ActorTurnEvent(Actor actor, int turnId, int time)
                => (Actor, TurnId, Time) = (actor, turnId, time);
        }
    }
}
