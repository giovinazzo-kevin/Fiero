namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ScrollReadEvent
        {
            public readonly Actor Actor;
            public readonly Scroll Scroll;
            public ScrollReadEvent(Actor actor, Scroll scroll)
                => (Actor, Scroll) = (actor, scroll);
        }
    }
}
