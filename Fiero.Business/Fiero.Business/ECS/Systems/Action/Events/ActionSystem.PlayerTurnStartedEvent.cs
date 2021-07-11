namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct PlayerTurnStartedEvent
        {
            public readonly int PlayerId;
            public readonly int TurnId;
            public PlayerTurnStartedEvent(int playerId, int turnId)
                => (PlayerId, TurnId) = (playerId, turnId);
        }
    }
}
