using SFML.Window;
using System;

namespace Fiero.Core
{
    public class Widget : UIWindow
    {
        private Coord? _dragStart;

        public bool EnableDragging { get; set; } = true;

        public event Action<Widget, Coord> Dragged;
        public event Action<Widget, Coord> Dropped;

        protected Widget(GameUI ui) : base(ui)
        {
        }

        public override void Update()
        {
            if (!IsOpen)
                return;
            base.Update();
            if (!EnableDragging)
                return;
            var mousePos = UI.Input.GetMousePosition();
            var leftClick = UI.Input.IsButtonPressed(Mouse.Button.Left);
            var leftDown = UI.Input.IsButtonDown(Mouse.Button.Left);
            var gameWindowSize = UI.Window.Size;
            if (_dragStart.HasValue && leftDown)
            {
                Layout.Position.V = (mousePos - _dragStart.Value);
                // Calculate by how much the window is offscreen and correct by that much
                var offscreen = ((Layout.Position.V + Size.V) - gameWindowSize);
                Layout.Position.V -= offscreen;

                Dragged?.Invoke(this, Layout.Position.V);
            }
            else if (Layout.Contains(mousePos, out _) && !_dragStart.HasValue && leftClick)
            {
                _dragStart = mousePos - Layout.Position.V;
            }
            else if (!leftDown && _dragStart.HasValue)
            {
                _dragStart = null;
                Dropped?.Invoke(this, Layout.Position.V);
            }
        }

        public override LayoutGrid CreateLayout(LayoutGrid grid, string title) => ApplyStyles(grid)
            .Col()
                .Repeat(1, (i, g) => RenderContent(g))
            .End();
    }
}
