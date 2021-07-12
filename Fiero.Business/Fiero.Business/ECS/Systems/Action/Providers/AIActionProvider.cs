using Fiero.Core;
using System;
using System.Linq;

namespace Fiero.Business
{
    public class AIActionProvider : ActionProvider
    {
        public override IAction GetIntent(Actor a)
        {
            return Act(a);
            static IAction Act(Actor a)
            {
                if (a.AI.Target is { Id: 0 }) {
                    a.AI.Target = null; // invalidation
                }
                if (a.AI.Target == null) {
                    // Seek new target to attack
                    var target = a.ActorProperties.CurrentFloor.Actors
                        .Where(b => a.IsHotileTowards(b))
                        .Select(b => (Actor: b, Dist: b.DistanceFrom(a)))
                        .Where(t => t.Dist < 10 && a.CanSee(t.Actor))
                        .OrderBy(t => t.Dist)
                        .Select(t => t.Actor)
                        .FirstOrDefault();
                    if (target != null) {
                        a.AI.Target = target;
                    }
                }
                if (a.AI.Target != null) {
                    if (a.AI.Target.DistanceFrom(a) < 2) {
                        return new AttackOtherAction(a.AI.Target);
                    }
                    if (a.CanSee(a.AI.Target)) {
                        // If we can see the target and it has moved, recalculate the path as to remember its last position
                        a.AI.Path = a.ActorProperties.CurrentFloor.Pathfinder.Search(a.Physics.Position, a.AI.Target.Physics.Position, default);
                        a.AI.Path?.RemoveFirst();
                    }
                }
                if (a.AI.Path != null) {
                    if (a.AI.Path.First != null) {
                        var pos = a.AI.Path.First.Value.Physics.Position;
                        var dir = new Coord(pos.X - a.Physics.Position.X, pos.Y - a.Physics.Position.Y);
                        var diff = Math.Abs(dir.X) + Math.Abs(dir.Y);
                        a.AI.Path.RemoveFirst();
                        if (diff > 0 && diff <= 2) {
                            // one tile ahead
                            return new MoveRelativeAction(dir);
                        }
                    }
                    else {
                        a.AI.Path = null;
                        return Act(a);
                    }
                }
                return new MoveRandomlyAction();
            }
        }
    }
}
