namespace Fiero.Business
{
    public partial class DungeonSystem
    {
        public readonly record struct ActorChangedEvent(Floor Floor, Actor OldState, Actor NewState);
    }
}
