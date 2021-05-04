using Fiero.Core;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Fiero.Business
{
    public static class ActionProvider
    {
        public static Func<Actor, ActionName> EnemyAI() => a => {
            return Act(a);
            static ActionName Act(Actor a)
            {
                if(a.Action.Target is { Id: 0 }) {
                    a.Action.Target = null; // invalidation
                }
                if (a.Action.Target == null) {
                    // Seek new target to attack
                    var target = a.Properties.CurrentFloor.Actors
                        .Where(b => a.IsHotileTowards(b))
                        .Select(b => (Actor: b, Dist: b.DistanceFrom(a)))
                        .Where(t => t.Dist < 10 && a.CanSee(t.Actor))
                        .OrderBy(t => t.Dist)
                        .Select(t => t.Actor)
                        .FirstOrDefault();
                    if (target != null) {
                        a.Action.Target = target;
                    }
                }
                if(a.Action.Target != null) {
                    if (a.Action.Target.DistanceFrom(a) < 2) {
                        return ActionName.Attack;
                    }
                    if (a.CanSee(a.Action.Target)) {
                        // If we can see the target and it has moved, recalculate the path as to remember its last position
                        a.Action.Path = a.Properties.CurrentFloor.Pathfinder.Search(a.Physics.Position, a.Action.Target.Physics.Position, default);
                        a.Action.Path?.RemoveFirst();
                    }
                }
                if (a.Action.Path != null) {
                    if (a.Action.Path.First != null) {
                        var pos = a.Action.Path.First.Value.Physics.Position;
                        var dir = new Coord(pos.X - a.Physics.Position.X, pos.Y - a.Physics.Position.Y);
                        var diff = Math.Abs(dir.X) + Math.Abs(dir.Y);
                        a.Action.Path.RemoveFirst();
                        if (diff > 0 && diff <= 2) {
                            // one tile ahead
                            a.Action.Direction = dir;
                            return ActionName.Move;
                        }
                    }
                    else {
                        a.Action.Path = null;
                        return Act(a);
                    }
                }
                a.Action.Direction = null;
                return ActionName.Move;
            }
        };

        public static Func<Actor, ActionName> PlayerInput(GameInput input) => a => {
            var moveIntent = input.IsKeyDown(SFML.Window.Keyboard.Key.LControl)
                ? ActionName.Attack
                : ActionName.Move;
            a.Action.Target = null;
            a.Action.Direction = null;
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad7)) {
                a.Action.Direction = new(-1, -1);
                return moveIntent;
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad8)) {
                a.Action.Direction = new(0, -1);
                return moveIntent;
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad9)) {
                a.Action.Direction = new(1, -1);
                return moveIntent;
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad4)) {
                a.Action.Direction = new(-1, 0);
                return moveIntent;
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad5)) {
                a.Action.Direction = new(0, 0);
                return moveIntent;
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad6)) {
                a.Action.Direction = new(1, 0);
                return moveIntent;
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad1)) {
                a.Action.Direction = new(-1, 1);
                return moveIntent;
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad2)) {
                a.Action.Direction = new(0, 1);
                return moveIntent;
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.Numpad3)) {
                a.Action.Direction = new(1, 1);
                return moveIntent;
            }
            if (input.IsKeyPressed(SFML.Window.Keyboard.Key.G)) {
                a.Action.Direction = new(0, 0);
                return ActionName.Use;
            }
            return ActionName.None;
        };
    }
}
