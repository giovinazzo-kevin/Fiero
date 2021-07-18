using Fiero.Core;

namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorMovedEvent
        {
            public readonly Actor Actor;
            public readonly Coord OldPosition;
            public readonly Coord NewPosition;
            public ActorMovedEvent(Actor actor, Coord oldPos, Coord newPos)
                => (Actor, OldPosition, NewPosition) = (actor, oldPos, newPos);
        }
    }
}
