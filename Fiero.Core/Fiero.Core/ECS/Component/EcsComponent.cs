namespace Fiero.Core
{
    public abstract class EcsComponent
    {
        public int Id { get; internal set; }
        public int EntityId { get; internal set; }
    }
}
