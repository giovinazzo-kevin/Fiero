namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct ActorBumpedObstacleEvent
        {
            public readonly Actor Actor;
            public readonly Drawable Obstacle;
            public ActorBumpedObstacleEvent(Actor actor, Drawable obstacle)
                => (Actor, Obstacle) = (actor, obstacle);
        }
    }
}
