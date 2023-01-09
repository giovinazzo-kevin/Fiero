namespace Fiero.Business
{
    public partial class DungeonSystem
    {
        public readonly struct TileChangedEvent
        {
            public readonly Floor Floor;
            public readonly Tile OldState;
            public readonly Tile NewState;
            public TileChangedEvent(Floor floor, Tile oldState, Tile newState)
                => (Floor, OldState, NewState) = (floor, oldState, newState);
        }
    }
}
