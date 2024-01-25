using SFML.Graphics;
using SFML.Window;

namespace Fiero.Core
{
    public sealed class GameWindow
    {
        private RenderWindow _window;
        public RenderWindow RenderWindow
        {
            get => _window; internal set
            {
                var old = _window;
                _window = value;
                RenderWindowChanged?.Invoke(this, old);
            }
        }
        public Coord Size => RenderWindow?.Size.ToCoord() ?? Coord.Zero;
        public GameWindow()
        {
        }
        public bool HasFocus() => RenderWindow.HasFocus();
        public void WaitAndDispatchEvents() => RenderWindow.WaitAndDispatchEvents();
        public void DispatchEvents() => RenderWindow.DispatchEvents();
        public void Display() => RenderWindow.Display();
        public void Clear() => RenderWindow.Clear(Color.Transparent);
        public void Clear(Color c) => RenderWindow.Clear(c);
        public void Draw(Drawable d) => RenderWindow.Draw(d);
        public void Draw(Drawable d, RenderStates states) => RenderWindow.Draw(d, states);
        public Coord GetMousePosition() => Mouse.GetPosition(RenderWindow).ToCoord();

        public event Action<GameWindow, RenderWindow> RenderWindowChanged;
    }
}
