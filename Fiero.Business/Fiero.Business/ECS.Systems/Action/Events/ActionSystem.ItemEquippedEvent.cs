namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ItemEquippedEvent(Actor Actor, Item Item);
    }
}
