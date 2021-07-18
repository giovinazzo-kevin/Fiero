namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ItemEquippedEvent
        {
            public readonly Actor Actor;
            public readonly Item Item;
            public ItemEquippedEvent(Actor actor, Item item)
                => (Actor, Item) = (actor, item);
        }
    }
}
