namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ItemConsumedEvent(Actor Actor, Item Item);
    }
}
