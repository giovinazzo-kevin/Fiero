namespace Fiero.Core
{
    public abstract class Component
    {
        public int Id { get; internal set; }
        public int EntityId { get; internal set; }
    }
}
