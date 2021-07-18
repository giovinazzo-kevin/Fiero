namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorDiedEvent
        {
            public readonly Actor Actor;
            public ActorDiedEvent(Actor actor)
                => (Actor) = (actor);
        }
    }
}
