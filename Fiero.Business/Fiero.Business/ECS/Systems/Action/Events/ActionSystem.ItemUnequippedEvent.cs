namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ItemUnequippedEvent
        {
            public readonly Actor Actor;
            public readonly Item Item;
            public ItemUnequippedEvent(Actor actor, Item item)
                => (Actor, Item) = (actor, item);
        }
    }
}
