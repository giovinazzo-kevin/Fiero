namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ItemDroppedEvent
        {
            public readonly Actor Actor;
            public readonly Item Item;
            public ItemDroppedEvent(Actor actor, Item item)
                => (Actor, Item) = (actor, item);
        }
    }
}
