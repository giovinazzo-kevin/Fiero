using Fiero.Core;
using System;
using System.Linq;

namespace Fiero.Business
{
    public class AIActionProvider : ActionProvider
    {
        protected readonly GameSystems Systems;

        public AIActionProvider(GameSystems systems)
        {
            Systems = systems;
        }

        public override IAction GetIntent(Actor a)
        {
            return new MoveRandomlyAction();
            if (a.AI.Target is { Id: 0 }) {
                a.AI.Target = null; // invalidation
            }
            // If following a path, do so until the end or an obstacle is reached
            if (a.AI.Path != null) {
                if (a.AI.Path.First != null) {
                    var pos = a.AI.Path.First.Value.Tile.Physics.Position;
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
                    return GetIntent(a);
                }
            }
            // If wandering aimlessly, seek a new target (this is expensive so only do it occasionally)
            if (a.AI.Target == null) {
                // Seek new target to attack
                var target = Systems.Floor.GetAllActors(a.ActorProperties.FloorId)
                    .Where(b => a.IsHostileTowards(b))
                    .FirstOrDefault();
                if (target != null) {
                    a.AI.Target = target;
                }
            }
            if (a.AI.Target != null) {
                if (a.AI.Target.DistanceFrom(a) < 2) {
                    return new MeleeAttackOtherAction(a.AI.Target);
                }
                if (Systems.Floor.CanSee(a, a.AI.Target) && Systems.Floor.TryGetFloor(a.ActorProperties.FloorId, out var floor)) {
                    // If we can see the target and it has moved, recalculate the path as to remember its last position
                    a.AI.Path = floor.Pathfinder.Search(a.Physics.Position, a.AI.Target.Physics.Position, default);
                    a.AI.Path?.RemoveFirst();
                }
            }
            return new MoveRandomlyAction();
        }
    }
}
