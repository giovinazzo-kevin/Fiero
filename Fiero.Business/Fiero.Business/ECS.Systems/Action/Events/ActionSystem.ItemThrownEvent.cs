using Fiero.Core;

namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ItemThrownEvent
        {
            public readonly Actor Actor;
            public readonly Actor Victim;
            public readonly Coord Position;
            public readonly Throwable Item;
            public ItemThrownEvent(Actor actor, Actor victim, Coord pos, Throwable item)
                => (Actor, Victim, Position, Item) = (actor, victim, pos, item);
        }
    }
}
