using System;
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
