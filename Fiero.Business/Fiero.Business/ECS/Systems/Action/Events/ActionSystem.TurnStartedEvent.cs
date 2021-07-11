namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct TurnStartedEvent
        {
            public readonly int TurnId;
            public TurnStartedEvent(int turnId) 
                => TurnId = turnId;
        }
    }
}
