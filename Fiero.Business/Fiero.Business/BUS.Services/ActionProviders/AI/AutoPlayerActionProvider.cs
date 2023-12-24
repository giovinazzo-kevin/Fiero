namespace Fiero.Business
{
    [TransientDependency]
    public class AutoPlayerActionProvider : AiActionProvider
    {
        protected readonly AiSensor<Actor> EnemiesOnFloor;

        public AutoPlayerActionProvider(MetaSystem systems) : base(systems)
        {
            Sensors.Add(
                EnemiesOnFloor = new((sys, a) =>
                {
                    return sys.Get<DungeonSystem>().GetAllActors(a.FloorId())
                         .Where(b => sys.Get<FactionSystem>().GetRelations(a, b).Right.IsHostile());
                }));
            RepathChance = Chance.Always;
        }

        protected override void Repath(Actor a)
        {
            if (a.Ai.Target == null)
            {
                Console.WriteLine("Recalc");
            }
            base.Repath(a);
        }
    }
}
