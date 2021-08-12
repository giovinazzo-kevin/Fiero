using Fiero.Core;
using LightInject;
using SFML.Window;
using System;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace Fiero.Business
{
    public static class GameUIExtensions
    {
        public static InventoryModal Inventory(this GameUI ui, Actor actor, string title = null)
            => ui.ShowModal(
                new InventoryModal(ui, ui.ServiceProvider.GetInstance<GameResources>(), actor),
                title,
                new[] { ModalWindowButton.Close },
                title != null ? ModalWindowStyles.Default
                              : ModalWindowStyles.Default & ~ModalWindowStyles.Title
            );
        public static DialogueModal Dialogue(this GameUI ui, IDialogueTrigger trigger, DialogueNode node, DrawableEntity speaker, params DrawableEntity[] listeners)
            => ui.ShowModal(
                new DialogueModal(ui, ui.ServiceProvider.GetInstance<GameResources>(), trigger, node, speaker, listeners),
                null,
                new[] { ModalWindowButton.Ok },
                ModalWindowStyles.None
            );
        public static ChoicePopUp<T> NecessaryChoice<T>(this GameUI ui, T[] choices, string title = null)
            => ui.ShowModal(
                new ChoicePopUp<T>(ui, ui.ServiceProvider.GetInstance<GameResources>(), choices), 
                title, 
                new[] { ModalWindowButton.Ok },
                title != null ? ModalWindowStyles.Default
                              : ModalWindowStyles.Default & ~ModalWindowStyles.Title
            );
        public static ChoicePopUp<T> OptionalChoice<T>(this GameUI ui, T[] choices, string title = null)
            => ui.ShowModal(
                new ChoicePopUp<T>(ui, ui.ServiceProvider.GetInstance<GameResources>(), choices), 
                title, 
                new[] { ModalWindowButton.Ok, ModalWindowButton.Cancel },
                title != null ? ModalWindowStyles.Default
                              : ModalWindowStyles.Default & ~ModalWindowStyles.Title
            );

        internal static bool FreeCursor(this GameUI ui,
            TargetingShape shape,
            out TargetingShape finalShape,
            Action<TargetingShape> cursorMoved = null
        )
        {
            var gameLoop = (GameLoop)ui.ServiceProvider.GetInstance(typeof(GameLoop));
            var renderSystem = (RenderSystem)ui.ServiceProvider.GetInstance(typeof(RenderSystem));
            var result = false;
            finalShape = TargetingShape.Offset(shape, renderSystem.Screen.GetViewportCenter());
            renderSystem.Screen.ShowTargetingShape(finalShape);
            cursorMoved?.Invoke(renderSystem.Screen.GetTargetingShape());
            gameLoop.LoopAndDraw(() => {
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.Cancel))) {
                    result = false;
                    return true;
                }
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.Confirm))) {
                    result = true;
                    return true;
                }
                return false;
            }, (t, dt) => {
                ui.Window.DispatchEvents();
                ui.Input.Update(ui.Window.GetMousePosition());
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.MoveN)))
                    Move(new(0, -1));
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.MoveS)))
                    Move(new(0, 1));
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.MoveW)))
                    Move(new(-1, 0));
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.MoveE)))
                    Move(new(1, 0));
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.MoveNW)))
                    Move(new(-1, -1));
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.MoveNE)))
                    Move(new(1, -1));
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.MoveSW)))
                    Move(new(-1, 1));
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.MoveSE)))
                    Move(new(1, 1));
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.RotateTargetCW)))
                    Rotate(15);
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.RotateTargetCCW)))
                    Rotate(-15);
            });
            finalShape = renderSystem.Screen.GetTargetingShape();
            renderSystem.Screen.HideTargetingShape();
            return result;

            void Move(Coord c)
            {
                if (renderSystem.Screen.GetTargetingShape().TryOffset(c, out var offset)) {
                    renderSystem.Screen.ShowTargetingShape(offset);
                    cursorMoved?.Invoke(offset);
                }
            }

            void Rotate(int deg)
            {
                if (renderSystem.Screen.GetTargetingShape().TryRotate(deg, out var rotated)) {
                    renderSystem.Screen.ShowTargetingShape(rotated);
                    cursorMoved?.Invoke(rotated);
                }
            }
        }

        internal static bool TargetedCursor(this GameUI ui,
            out Coord cursorPos,
            Coord[] allowedPositions,
            Action<Coord> cursorMoved = null
        )
        {
            var gameLoop = (GameLoop)ui.ServiceProvider.GetInstance(typeof(GameLoop));
            var renderSystem = (RenderSystem)ui.ServiceProvider.GetInstance(typeof(RenderSystem));
            var result = false;
            cursorPos = allowedPositions.First();
            renderSystem.Screen.ShowTargetingShape(new(4, false, cursorPos));
            cursorMoved?.Invoke(renderSystem.Screen.GetTargetingShape().Points.Single());
            gameLoop.LoopAndDraw(() => {
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.Cancel))) {
                    result = false;
                    return true;
                }
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.Confirm))) {
                    result = true;
                    return true;
                }
                return false;
            }, (t, dt) => {
                ui.Window.DispatchEvents();
                ui.Input.Update(ui.Window.GetMousePosition());
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.MoveW)))
                    Move(-1);
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.MoveE)))
                    Move(+1);
            });
            cursorPos = renderSystem.Screen.GetTargetingShape().Points.Single();
            renderSystem.Screen.HideTargetingShape();
            return result;

            void Move(int i)
            {
                var pos = renderSystem.Screen.GetTargetingShape().Points.Single();
                var index = Array.IndexOf(allowedPositions, pos);
                var newPos = allowedPositions[(index + i).Mod(allowedPositions.Length)];
            renderSystem.Screen.ShowTargetingShape(new(1000, false, newPos));
                cursorMoved?.Invoke(renderSystem.Screen.GetTargetingShape().Points.Single());
            }
        }

        public static bool Look(this GameUI ui, out TargetingShape cursorPos)
        {
            var floorSystem = (FloorSystem)ui.ServiceProvider.GetInstance(typeof(FloorSystem));
            var renderSystem = (RenderSystem)ui.ServiceProvider.GetInstance(typeof(RenderSystem));
            var shape = new TargetingShape(1000, false, new Coord());
            var result = ui.FreeCursor(shape, out cursorPos, cursorMoved: c => {
                var floorId = renderSystem.Screen.GetViewportFloor();
                if (floorSystem.TryGetCellAt(floorId, c.Points.Single(), out var cell)) {
                    renderSystem.Screen.SetLookText(cell.ToString());
                }
                else {
                    renderSystem.Screen.SetLookText(String.Empty);
                }
            });
            renderSystem.Screen.SetLookText(String.Empty);
            return result;
        }

        public static bool OldTarget(this GameUI ui, Coord[] allowedPositions, out Coord cursorPos)
        {
            var floorSystem = (FloorSystem)ui.ServiceProvider.GetInstance(typeof(FloorSystem));
            var renderSystem = (RenderSystem)ui.ServiceProvider.GetInstance(typeof(RenderSystem));
            var result = ui.TargetedCursor(out cursorPos, allowedPositions, cursorMoved: c => {
                var floorId = renderSystem.Screen.GetViewportFloor();
                if (floorSystem.TryGetCellAt(floorId, c, out var cell)) {
                    renderSystem.Screen.SetLookText(cell.ToString());
                }
                else {
                    renderSystem.Screen.SetLookText(String.Empty);
                }
            });
            renderSystem.Screen.SetLookText(String.Empty);
            return result;
        }

        public static bool Target(this GameUI ui, TargetingShape shape, out TargetingShape finalShape)
        {
            var floorSystem = (FloorSystem)ui.ServiceProvider.GetInstance(typeof(FloorSystem));
            var renderSystem = (RenderSystem)ui.ServiceProvider.GetInstance(typeof(RenderSystem));
            var result = ui.FreeCursor(shape, out finalShape);
            return result;
        }
    }
}
