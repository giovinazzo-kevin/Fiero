namespace Fiero.Business
{
    public partial class InterfaceSystem
    {
        public readonly struct ActorSelectedEvent
        {
            public readonly Actor Actor;
            public ActorSelectedEvent(Actor a)
                => (Actor) = (a);
        }
    }
}
