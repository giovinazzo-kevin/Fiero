using Fiero.Core;
using System.Linq;

namespace Fiero.Business
{
    public abstract class ActionProvider
    {
        public readonly GameSystems Systems;

        public ActionProvider(GameSystems sys)
        {
            Systems = sys;
        }

        public abstract IAction GetIntent(Actor actor);
        public abstract bool TryTarget(Actor a, TargetingShape shape, bool autotargetSuccesful);

        protected bool TryZap(Actor a, Wand wand, out IAction action)
        {
            var floorId = a.FloorId();
            var flags = wand.GetEffectFlags();
            // All wands use the same targeting shape and have "infinite" range
            var line = Shapes.Line(new(0, 0), new(0, 100)).Skip(1).ToArray();
            var zapShape = new RayTargetingShape(a.Position(), 100);
            var autoTarget = zapShape.TryAutoTarget(
                p => Systems.Dungeon.GetActorsAt(floorId, p).Any(b =>
                {
                    if (!wand.ItemProperties.Identified)
                        return true;
                    var rel = Systems.Faction.GetRelations(a, b);
                    if (wand.Effects != null && wand.Effects.Intrinsic.All(e => b.Effects.Active.Any(f => f.Name == e.Name)))
                        return false;
                    if (rel.Left.IsFriendly() && flags.IsDefensive)
                        return true;
                    if (rel.Left.IsHostile() && flags.IsOffensive)
                        return true;
                    return false;
                }),
                p => !Systems.Dungeon.GetCellAt(floorId, p)?.IsWalkable(a) ?? true
            );
            if (TryTarget(a, zapShape, autoTarget))
            {
                var points = zapShape.GetPoints().ToArray();
                foreach (var p in points)
                {
                    var target = Systems.Dungeon.GetActorsAt(floorId, p)
                        .FirstOrDefault();
                    if (target != null)
                    {
                        action = new ZapWandAtOtherAction(wand, target);
                        return true;
                    }
                }
                // Okay, then
                action = new ZapWandAtPointAction(wand, points.Last() - a.Position());
                return true;
            }
            action = default;
            return false;
        }


        protected bool TryThrow(Actor a, Throwable throwable, out IAction action)
        {
            var floorId = a.FloorId();
            var len = throwable.ThrowableProperties.MaximumRange + 1;
            var line = Shapes.Line(new(0, 0), new(0, len))
                .Skip(1)
                .ToArray();
            var flags = throwable.GetEffectFlags();
            var throwShape = new RayTargetingShape(a.Position(), len);
            var autoTarget = throwShape.TryAutoTarget(
                p => Systems.Dungeon.GetActorsAt(floorId, p).Any(b =>
                {
                    if (!throwable.ItemProperties.Identified)
                        return true;
                    var rel = Systems.Faction.GetRelations(a, b);
                    if (throwable.Effects != null && throwable.Effects.Intrinsic.All(e => b.Effects.Active.Any(f => f.Name == e.Name)))
                        return false;
                    if (rel.Left.IsFriendly() && flags.IsDefensive)
                        return true;
                    if (rel.Left.IsHostile() && flags.IsOffensive)
                        return true;
                    if (rel.Left.IsHostile() && throwable.ThrowableProperties.BaseDamage > 0)
                        return true;
                    return false;
                }),
                p => !Systems.Dungeon.GetCellAt(floorId, p)?.IsWalkable(a) ?? true
            );
            if (TryTarget(a, throwShape, autoTarget))
            {
                var points = throwShape.GetPoints().ToArray();
                foreach (var p in points)
                {
                    var target = Systems.Dungeon.GetActorsAt(floorId, p)
                        .FirstOrDefault(b => Systems.Faction.GetRelations(a, b).Left.IsHostile());
                    if (target != null)
                    {
                        action = new ThrowItemAtOtherAction(target, throwable);
                        return true;
                    }
                }
                // Okay, then
                action = new ThrowItemAtPointAction(points.Last() - a.Position(), throwable);
                return true;
            }
            action = default;
            return false;
        }
    }
}
