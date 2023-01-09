namespace Fiero.Business
{
    public partial class RenderSystem
    {
        public readonly struct ActorSelectedEvent
        {
            public readonly Actor Actor;
            public ActorSelectedEvent(Actor a)
                => (Actor) = (a);
        }
    }
}
