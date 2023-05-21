using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    /// <summary>
    /// Merges all corridors into a single layer and draws them properly, such that no connector is drawn twice and such that only the largest door-frame is drawn.
    /// </summary>
    public class CorridorLayer : IFloorGenerationPrefab
    {
        public readonly HashSet<Corridor> Corridors;

        public CorridorLayer(IEnumerable<Corridor> corridors)
        {
            Corridors = corridors.ToHashSet();
        }

        public void Draw(FloorGenerationContext ctx)
        {
            var allConnectors = Corridors
                .SelectMany(c => new[] { c.Start, c.End })
                .Distinct();
            foreach (var corridor in Corridors)
            {
                corridor.DrawPoints(ctx);
            }
            foreach (var connector in allConnectors)
            {
                var start = Corridors
                    .Where(x => x.Start == connector)
                    .MaxBy(x => x.EffectiveStartThickness);
                var end = Corridors
                    .Where(x => x.End == connector)
                    .MaxBy(x => x.EffectiveEndThickness);
                var piece = (start?.EffectiveStartThickness ?? -1) > (end?.EffectiveEndThickness ?? -1)
                    ? start : end;
                piece?.DrawDoors(ctx, start: piece == start, end: piece == end);
            }
        }
    }
}
