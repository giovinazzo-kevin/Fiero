using Fiero.Core;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Fiero.Business
{

    public static class ActionProvider
    {
        public static Func<Actor, IAction> EnemyAI() => a => {
            return Act(a);
            static IAction Act(Actor a)
            {
                if(a.AI.Target is { Id: 0 }) {
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
                if(a.AI.Target != null) {
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
        };

        public static Func<Actor, IAction> PlayerInput(GameInput input) => a => {
            var moveIntent = input.IsKeyDown(SFML.Window.Keyboard.Key.LControl)
                ? ActionName.Attack
                : ActionName.Move;
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad7)) {
                return new MoveRelativeAction(new(-1, -1));
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad8)) {
                return new MoveRelativeAction(new(0, -1));
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad9)) {
                return new MoveRelativeAction(new(1, -1));
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad4)) {
                return new MoveRelativeAction(new(-1, 0));
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad5)) {
                return new MoveRelativeAction(new(0, 0));
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad6)) {
                return new MoveRelativeAction(new(1, 0));
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad1)) {
                return new MoveRelativeAction(new(-1, 1));
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad2)) {
                return new MoveRelativeAction(new(0, 1));
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad3)) {
                return new MoveRelativeAction(new(1, 1));
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.G)) {
                return new InteractRelativeAction();
            }
            return new NoAction();
        };
    }
}
