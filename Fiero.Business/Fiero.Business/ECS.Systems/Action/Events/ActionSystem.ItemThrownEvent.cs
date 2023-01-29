using Fiero.Core;

namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct ItemThrownEvent(Actor Actor, Actor Victim, Coord Position, Throwable Item);
    }
}
