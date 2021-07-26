namespace Fiero.Business
{
    public partial class FloorSystem
    {
        public readonly struct FeatureChangedEvent
        {
            public readonly Floor Floor;
            public readonly Feature OldState;
            public readonly Feature NewState;
            public FeatureChangedEvent(Floor floor, Feature oldState, Feature newState)
                => (Floor, OldState, NewState) = (floor, oldState, newState);
        }
    }
}
