namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ItemDroppedEvent(Actor Actor, Item Item);
    }
}
