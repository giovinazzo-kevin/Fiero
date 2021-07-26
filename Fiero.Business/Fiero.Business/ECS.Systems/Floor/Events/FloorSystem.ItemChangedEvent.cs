namespace Fiero.Business
{
    public partial class FloorSystem
    {
        public readonly struct ItemChangedEvent
        {
            public readonly Floor Floor;
            public readonly Item OldState;
            public readonly Item NewState;
            public ItemChangedEvent(Floor floor, Item oldState, Item newState)
                => (Floor, OldState, NewState) = (floor, oldState, newState);
        }
    }
}
