namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorIntentEvent
        {
            public readonly Actor Actor;
            public readonly IAction Intent;
            public readonly int TurnId;
            public readonly int Time;
            public ActorIntentEvent(Actor actor, IAction intent, int turnId, int time)
                => (Actor, Intent, TurnId, Time) = (actor, intent, turnId, time);
        }
    }
}
