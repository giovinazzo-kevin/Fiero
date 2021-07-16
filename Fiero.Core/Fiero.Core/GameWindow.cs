using SFML.Graphics;
using SFML.Window;

namespace Fiero.Core
{
    public sealed class GameWindow
    {
        public RenderWindow RenderWindow { get; internal set; }
        public Coord Size => RenderWindow.Size.ToCoord();
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
    }
}
