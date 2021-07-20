using Fiero.Core;
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
                new InventoryModal(ui, actor),
                title,
                new[] { ModalWindowButton.Close },
                title != null ? ModalWindowStyles.Default
                              : ModalWindowStyles.Default & ~ModalWindowStyles.Title
            );
        public static DialogueModal Dialogue(this GameUI ui, IDialogueTrigger trigger, DialogueNode node, Drawable speaker, params Drawable[] listeners)
            => ui.ShowModal(
                new DialogueModal(ui, trigger, node, speaker, listeners),
                null,
                new[] { ModalWindowButton.Ok },
                ModalWindowStyles.None
            );
        public static ChoicePopUp<T> NecessaryChoice<T>(this GameUI ui, T[] choices, string title = null)
            => ui.ShowModal(
                new ChoicePopUp<T>(ui, choices), 
                title, 
                new[] { ModalWindowButton.Ok },
                title != null ? ModalWindowStyles.Default
                              : ModalWindowStyles.Default & ~ModalWindowStyles.Title
            );
        public static ChoicePopUp<T> OptionalChoice<T>(this GameUI ui, T[] choices, string title = null)
            => ui.ShowModal(
                new ChoicePopUp<T>(ui, choices), 
                title, 
                new[] { ModalWindowButton.Ok, ModalWindowButton.Cancel },
                title != null ? ModalWindowStyles.Default
                              : ModalWindowStyles.Default & ~ModalWindowStyles.Title
            );

        public static bool Cursor(this GameUI ui, out Coord cursorPos, Coord? cursorInitialPos = null, Action<Coord> cursorMoved = null)
        {
            var gameLoop = (GameLoop)ui.ServiceProvider.GetInstance(typeof(GameLoop));
            var renderSystem = (RenderSystem)ui.ServiceProvider.GetInstance(typeof(RenderSystem));
            var result = false;
            cursorPos = cursorInitialPos ?? renderSystem.GetViewportCenter();
            renderSystem.ShowCursor(cursorPos);
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
            });
            cursorPos = renderSystem.GetCursorPosition();
            renderSystem.HideCursor();
            return result;

            void Move(Coord c)
            {
                renderSystem.MoveCursor(c);
                cursorMoved?.Invoke(renderSystem.GetCursorPosition());
            }
        }

        public static bool Look(this GameUI ui, out Coord cursorPos)
        {
            var floorSystem = (FloorSystem)ui.ServiceProvider.GetInstance(typeof(FloorSystem));
            var renderSystem = (RenderSystem)ui.ServiceProvider.GetInstance(typeof(RenderSystem));
            var result = ui.Cursor(out cursorPos, cursorMoved: c => {
                var floorId = renderSystem.GetViewportFloor();
                if (floorSystem.TryGetCellAt(floorId, c, out var cell)) {
                    renderSystem.SetLookText(cell.ToString());
                }
                else {
                    renderSystem.SetLookText(String.Empty);
                }
            });
            renderSystem.SetLookText(String.Empty);
            return result;
        }
    }
}
