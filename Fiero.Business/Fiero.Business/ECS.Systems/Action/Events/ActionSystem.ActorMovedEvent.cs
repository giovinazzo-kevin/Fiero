using Fiero.Core;

namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ActorMovedEvent(Actor Actor, Coord OldPosition, Coord NewPosition);
    }
}
