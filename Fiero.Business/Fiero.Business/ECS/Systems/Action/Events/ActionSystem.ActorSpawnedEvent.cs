namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorSpawnedEvent
        {
            public readonly Actor Actor;
            public ActorSpawnedEvent(Actor actor)
                => (Actor) = (actor);
        }
    }
}
