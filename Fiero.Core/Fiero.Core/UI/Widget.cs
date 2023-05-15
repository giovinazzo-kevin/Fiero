using SFML.Window;
using System;

namespace Fiero.Core
{
    public class Widget : UIWindow
    {
        private Coord? _dragStart;

        public event Action<Widget, Coord> Dragged;
        public event Action<Widget, Coord> Dropped;

        protected Widget(GameUI ui, GameDatum<Coord> gameWindowSize) : base(ui, gameWindowSize)
        {
        }

        public override void Update()
        {
            base.Update();
            var mousePos = UI.Input.GetMousePosition();
            var leftClick = UI.Input.IsButtonPressed(Mouse.Button.Left);
            var leftDown = UI.Input.IsButtonDown(Mouse.Button.Left);
            if (_dragStart.HasValue && leftDown)
            {
                Layout.Position.V = (mousePos - _dragStart.Value);
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
