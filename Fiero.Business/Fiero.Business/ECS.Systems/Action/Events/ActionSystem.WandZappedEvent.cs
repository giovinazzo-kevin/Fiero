using Fiero.Core;

namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct WandZappedEvent
        {
            public readonly Actor Actor;
            public readonly Actor Victim;
            public readonly Coord Position;
            public readonly Wand Wand;
            public WandZappedEvent(Actor actor, Actor victim, Coord pos, Wand item)
                => (Actor, Victim, Position, Wand) = (actor, victim, pos, item);
        }
    }
}
