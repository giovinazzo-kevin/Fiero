namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ItemPickedUpEvent
        {
            public readonly Actor Actor;
            public readonly Item Item;
            public ItemPickedUpEvent(Actor actor, Item item)
                => (Actor, Item) = (actor, item);
        }
    }
}
