using Fiero.Core;
using LightInject;
using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Business
{
    public static class GameUIExtensions
    {
        public static Color GetColor(this GameUI ui, ColorName name) =>
            ui.ServiceProvider.GetInstance<GameColors<ColorName>>().Get(name);

        public static InventoryModal Inventory(this GameUI ui, Actor actor, string title = null)
            => ui.Show(
                new InventoryModal(ui, ui.ServiceProvider.GetInstance<GameResources>(), actor),
                title
            );
        public static ChestModal Chest(this GameUI ui, Feature feature, bool canTake, string title = null)
            => ui.Show(
                new ChestModal(ui, ui.ServiceProvider.GetInstance<GameResources>(), feature, canTake),
                title
            );
        public static DialogueModal Dialogue(this GameUI ui, IDialogueTrigger trigger, DialogueNode node, DrawableEntity speaker, params DrawableEntity[] listeners)
            => ui.Show(
                new DialogueModal(ui, ui.ServiceProvider.GetInstance<GameResources>(), trigger, node, speaker, listeners),
                node.Title
            );
        public static ChoicePopUp<T> NecessaryChoice<T>(this GameUI ui, T[] choices, string title = null)
            => ui.Show(
                new ChoicePopUp<T>(
                    ui,
                    ui.ServiceProvider.GetInstance<GameResources>(),
                    choices,
                    new[] { ModalWindowButton.Ok },
                    ModalWindowStyles.Default
                ),
                title
            );
        public static ChoicePopUp<T> OptionalChoice<T>(this GameUI ui, T[] choices, string title = null)
            => ui.Show(
                new ChoicePopUp<T>(
                    ui,
                    ui.ServiceProvider.GetInstance<GameResources>(),
                    choices,
                    new[] { ModalWindowButton.Ok, ModalWindowButton.Cancel },
                    ModalWindowStyles.Default
                ),
                title
            );

        internal static bool FreeCursor(this GameUI ui,
            TargetingShape shape,
            Action<TargetingShape> cursorMoved = null
        )
        {
            var gameLoop = (GameLoop)ui.ServiceProvider.GetInstance(typeof(GameLoop));
            var renderSystem = (RenderSystem)ui.ServiceProvider.GetInstance(typeof(RenderSystem));
            var result = false;
            renderSystem.ShowTargetingShape(shape);
            cursorMoved?.Invoke(renderSystem.GetTargetingShape());
            gameLoop.LoopAndDraw(() =>
            {
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.Cancel)))
                {
                    result = false;
                    return true;
                }
                if (ui.Input.IsKeyPressed(ui.Store.Get(Data.Hotkeys.Confirm)))
                {
                    result = true;
                    return true;
                }
                return false;
            }, (t, dt) =>
            {
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
            renderSystem.HideTargetingShape();
            return result;

            void Move(Coord c)
            {
                if (shape.TryOffset(c))
                {
                    cursorMoved?.Invoke(shape);
                }
            }

            void RotateCw()
            {
                if (shape.TryRotateCw())
                {
                    cursorMoved?.Invoke(shape);
                }
            }

            void RotateCCw()
            {
                if (shape.TryRotateCCw())
                {
                    cursorMoved?.Invoke(shape);
                }
            }
        }

        public static bool Look(this GameUI ui, Actor a)
        {
            var floorSystem = (DungeonSystem)ui.ServiceProvider.GetInstance(typeof(DungeonSystem));
            var renderSystem = (RenderSystem)ui.ServiceProvider.GetInstance(typeof(RenderSystem));
            var shape = new PointTargetingShape(renderSystem.GetViewportCenter(), 1000);
            var result = ui.FreeCursor(shape, cursorMoved: c =>
            {
                var floorId = renderSystem.GetViewportFloor();
                var pos = c.GetPoints().Single();
                renderSystem.CenterOn(pos);
                if (a.Fov is null || a.Fov.KnownTiles[floorId].Contains(pos))
                {
                    if (floorSystem.TryGetCellAt(floorId, pos, out var cell))
                    {
                        //renderSystem.SetLookText(cell.ToString(a.Fov is null || a.Fov.VisibleTiles[floorId].Contains(pos)));
                    }
                    else
                    {
                        //renderSystem.SetLookText(String.Empty);
                    }
                }
                else
                {
                    //renderSystem.SetLookText("???");
                }
            });
            //renderSystem.SetLookText(String.Empty);
            return result;
        }

        public static bool Target(this GameUI ui, TargetingShape shape)
        {
            return ui.FreeCursor(shape);
        }
    }
}
