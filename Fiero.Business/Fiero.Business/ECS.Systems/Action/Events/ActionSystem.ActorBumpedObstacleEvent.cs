namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorBumpedObstacleEvent
        {
            public readonly Actor Actor;
            public readonly PhysicalEntity Obstacle;
            public ActorBumpedObstacleEvent(Actor actor, PhysicalEntity obstacle)
                => (Actor, Obstacle) = (actor, obstacle);
        }
    }
}
