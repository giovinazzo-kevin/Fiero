namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ItemConsumedEvent
        {
            public readonly Actor Actor;
            public readonly Item Item;
            public ItemConsumedEvent(Actor actor, Item item)
                => (Actor, Item) = (actor, item);
        }
    }
}
