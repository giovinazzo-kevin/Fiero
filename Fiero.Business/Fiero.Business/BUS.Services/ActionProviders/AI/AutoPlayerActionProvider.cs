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

        public override IAction GetIntent(Actor a)
        {
            var intent = base.GetIntent(a);
            if (EnemiesOnFloor.Values.Count == 0)
            {
                var downstairs = Systems.Dungeon.GetFeaturesAt(a.FloorId(), a.Position())
                    .Where(f => f.FeatureProperties.Name == FeatureName.Downstairs)
                    .FirstOrDefault();
                if (downstairs != null)
                {
                    return new InteractWithFeatureAction(downstairs);
                }
            }
            return intent;
        }

        protected override IAction Wander(Actor a)
        {
            if (EnemiesOnFloor.Values.Count == 0 && a.Ai.Target == null)
            {
                var floorId = a.FloorId();
                var downstairs = Systems.Dungeon.GetAllFeatures(floorId)
                    .Where(f => f.FeatureProperties.Name == FeatureName.Downstairs)
                    .FirstOrDefault();
                if (downstairs != null)
                {
                    SetTarget(a, downstairs);
                }
            }

            return base.Wander(a);
        }
    }
}
