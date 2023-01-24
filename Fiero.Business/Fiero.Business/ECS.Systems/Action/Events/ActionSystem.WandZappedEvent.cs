using Fiero.Core;

namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct WandZappedEvent(Actor Actor, Actor Victim, Coord Position, Wand Wand);
    }
}
