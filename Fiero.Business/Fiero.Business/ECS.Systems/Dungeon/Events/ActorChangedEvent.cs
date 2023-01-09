namespace Fiero.Business
{
    public partial class DungeonSystem
    {
        public readonly struct ActorChangedEvent
        {
            public readonly Floor Floor;
            public readonly Actor OldState;
            public readonly Actor NewState;
            public ActorChangedEvent(Floor floor, Actor oldState, Actor newState)
                => (Floor, OldState, NewState) = (floor, oldState, newState);
        }
    }
}
