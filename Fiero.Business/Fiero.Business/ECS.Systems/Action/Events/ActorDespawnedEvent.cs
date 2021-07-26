namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorDespawnedEvent
        {
            public readonly Actor Actor;
            public ActorDespawnedEvent(Actor actor)
                => (Actor) = (actor);
        }
    }
}
