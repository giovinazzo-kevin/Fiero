namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ItemPickedUpEvent(Actor Actor, Item Item);
    }
}
