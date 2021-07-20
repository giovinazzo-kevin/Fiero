using Fiero.Core;
using System;
using System.Linq;

namespace Fiero.Business
{
    public class AiActionProvider : ActionProvider
    {
        protected readonly GameSystems Systems;

        public AiActionProvider(GameSystems systems)
        {
            Systems = systems;
        }

        public override IAction GetIntent(Actor a)
        {
            if (a.Ai.Target is { Id: 0 }) {
                a.Ai.Target = null; // invalidation
            }
            // If wandering aimlessly, seek a new target (this is expensive so only do it occasionally)
            if (a.Ai.Target == null) {
                // Seek new target to attack
                var floorId = a.FloorId();
                var target = a.Fov.VisibleTiles
                    .SelectMany(p => Systems.Floor.GetActorsAt(floorId, p))
                    .Where(b => a.IsHostileTowards(b))
                    .FirstOrDefault();
                if (target != null) {
                    a.Ai.Target = target;
                }
            }
            if (a.Ai.Target != null) {
                if (a.Ai.Target.DistanceFrom(a) < 2) {
                    return new MeleeAttackOtherAction(a.Ai.Target);
                }
                if (a.CanSee(a.Ai.Target) && Systems.Floor.TryGetFloor(a.ActorProperties.FloorId, out var floor)) {
                    // If we can see the target and it has moved, recalculate the path as to remember its last position
                    a.Ai.Path = floor.Pathfinder.Search(a.Physics.Position, a.Ai.Target.Physics.Position, default);
                    a.Ai.Path?.RemoveFirst();
                }
            }
            // If following a path, do so until the end or an obstacle is reached
            if (a.Ai.Path != null) {
                if (a.Ai.Path.First != null) {
                    var pos = a.Ai.Path.First.Value.Tile.Physics.Position;
                    var dir = new Coord(pos.X - a.Physics.Position.X, pos.Y - a.Physics.Position.Y);
                    var diff = Math.Abs(dir.X) + Math.Abs(dir.Y);
                    a.Ai.Path.RemoveFirst();
                    if (diff > 0 && diff <= 2) {
                        // one tile ahead
                        return new MoveRelativeAction(dir);
                    }
                }
                else {
                    a.Ai.Path = null;
                    return GetIntent(a);
                }
            }
            return new MoveRandomlyAction();
        }
    }
}
