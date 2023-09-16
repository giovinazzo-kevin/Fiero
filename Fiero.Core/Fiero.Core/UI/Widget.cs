using SFML.Window;

namespace Fiero.Core
{
    public abstract class Widget : UIWindow
    {
        private Coord? _dragStart;

        public bool EnableDragging { get; set; } = true;
        public bool IsDragging => _dragStart.HasValue;

        public event Action<Widget, Coord> Dragged;
        public event Action<Widget, Coord> Dropped;

        protected Widget(GameUI ui) : base(ui)
        {
        }

        public override void Update(TimeSpan t, TimeSpan dt)
        {
            if (!IsOpen)
                return;
            if (!IsDragging)
                base.Update(t, dt);
            if (!EnableDragging)
                return;
            var mousePos = UI.Input.GetMousePosition();
            var leftClick = UI.Input.IsButtonPressed(Mouse.Button.Left);
            var leftDown = UI.Input.IsButtonDown(Mouse.Button.Left);
            var gameWindowSize = UI.Window.Size;
            if (_dragStart.HasValue && leftDown)
            {
                var delta = (mousePos - _dragStart.Value);
                if (delta.X == 0 && delta.Y == 0)
                    return;
                Layout.Position.V = Layout.Position.V + delta;
                _dragStart = mousePos;
                // Calculate by how much the window is offscreen and correct by that much
                var offscreenBottomRight = ((Layout.Position.V)).Clamp(maxX: 0, maxY: 0);
                var offscreenTopLeft = ((Layout.Position.V + Layout.Size.V) - gameWindowSize).Clamp(minX: 0, minY: 0);
                Layout.Position.V -= offscreenTopLeft;
                Layout.Position.V -= offscreenBottomRight;

                Dragged?.Invoke(this, Layout.Position.V);
            }
            else if (!_dragStart.HasValue && leftClick)
            {
                var any = false;
                foreach (var con in Layout.Contains(mousePos))
                {
                    // Make sure user clicked on a non-interactive part of the window
                    if (con.IsInteractive.V)
                        return;
                    any = true;
                }
                if (any) _dragStart = mousePos;
            }
            else if (!leftDown && _dragStart.HasValue)
            {
                _dragStart = null;
                Dropped?.Invoke(this, Layout.Position.V);
            }
        }
    }
}
