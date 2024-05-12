namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct PlayerTurnEvent(int PlayerId, int TurnId, int Time);
    }
}
