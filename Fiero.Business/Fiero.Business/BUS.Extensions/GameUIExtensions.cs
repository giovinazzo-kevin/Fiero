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
            Action<TargetingShape> cursorMoved = null
        )
        {
            var gameLoop = (GameLoop)ui.ServiceProvider.GetInstance(typeof(GameLoop));
            var renderSystem = (RenderSystem)ui.ServiceProvider.GetInstance(typeof(RenderSystem));
            var result = false;
            renderSystem.Screen.ShowTargetingShape(shape);
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
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.RotateTargetCw))
                    || shape.CanRotateWithDirectionKeys() && ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.MoveE)))
                    RotateCw();
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.RotateTargetCCw))
                    || shape.CanRotateWithDirectionKeys() && ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.MoveW)))
                    RotateCCw();
            });
            renderSystem.Screen.HideTargetingShape();
            return result;

            void Move(Coord c)
            {
                if (shape.TryOffset(c)) {
                    cursorMoved?.Invoke(shape);
                }
            }

            void RotateCw()
            {
                if (shape.TryRotateCw()) {
                    cursorMoved?.Invoke(shape);
                }
            }

            void RotateCCw()
            {
                if (shape.TryRotateCCw()) {
                    cursorMoved?.Invoke(shape);
                }
            }
        }

        public static bool Look(this GameUI ui)
        {
            var floorSystem = (FloorSystem)ui.ServiceProvider.GetInstance(typeof(FloorSystem));
            var renderSystem = (RenderSystem)ui.ServiceProvider.GetInstance(typeof(RenderSystem));
            var shape = new PointTargetingShape(renderSystem.Screen.GetViewportCenter(), 1000);
            var result = ui.FreeCursor(shape, cursorMoved: c => {
                var floorId = renderSystem.Screen.GetViewportFloor();
                if (floorSystem.TryGetCellAt(floorId, c.GetPoints().Single(), out var cell)) {
                    renderSystem.Screen.SetLookText(cell.ToString());
                }
                else {
                    renderSystem.Screen.SetLookText(String.Empty);
                }
            });
            renderSystem.Screen.SetLookText(String.Empty);
            return result;
        }

        public static bool Target(this GameUI ui, TargetingShape shape)
        {
            return ui.FreeCursor(shape);
        }
    }
}
