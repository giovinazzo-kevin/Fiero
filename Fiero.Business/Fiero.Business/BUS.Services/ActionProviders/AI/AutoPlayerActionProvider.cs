using System.Linq;

namespace Fiero.Business
{
    public class AutoPlayerActionProvider : AiActionProvider
    {
        protected readonly AiSensor<Actor> EnemiesOnFloor;

        public AutoPlayerActionProvider(GameSystems systems) : base(systems)
        {
            Sensors.Add(
                EnemiesOnFloor = new((sys, a) =>
                {
                    return sys.Dungeon.GetAllActors(a.FloorId())
                         .Where(b => sys.Faction.GetRelations(a, b).Right.IsHostile());
                }));
            RepathOneTimeIn = 1;
        }

        protected override IAction Wander(Actor a)
        {
            if (a.Ai.Target == null)
            {
                var features = Systems.Dungeon.GetAllFeatures(a.FloorId());
                var door = features
                    .Where(f => f.FeatureProperties.Name == FeatureName.Door && a.Knows(f.Physics.Position) && f.Physics.BlocksMovement)
                    .FirstOrDefault();
                if (door != null)
                {
                    SetTarget(a, door, () => new InteractWithFeatureAction(door));
                }
                else if (EnemiesOnFloor.Values.Count == 0)
                {
                    var downstairs = features
                        .Where(f => f.FeatureProperties.Name == FeatureName.Downstairs)
                        .FirstOrDefault();
                    if (downstairs != null)
                    {
                        SetTarget(a, downstairs, () => new InteractWithFeatureAction(downstairs));
                    }
                }
            }

            return base.Wander(a);
        }
    }
}
