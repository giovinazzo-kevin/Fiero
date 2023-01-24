namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ItemUnequippedEvent(Actor Actor, Item Item);
    }
}
