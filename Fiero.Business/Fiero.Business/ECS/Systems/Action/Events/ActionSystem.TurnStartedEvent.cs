namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct TurnEvent
        {
            public readonly int TurnId;
            public TurnEvent(int turnId) 
                => TurnId = turnId;
        }
    }
}
