using SFML.Window;

namespace Fiero.Core
{
    public partial class UIControl
    {
        public event Func<UIControl, Coord, Mouse.Button, bool> Clicked;
        protected virtual bool OnClicked(Coord mousePos, Mouse.Button button) { return false; }
        public event Action<UIControl, Coord> MouseEntered;
        protected virtual bool OnMouseEntered(Coord mousePos) { IsMouseOver.V = true; return false; }
        public event Action<UIControl, Coord> MouseMoved;
        protected virtual bool OnMouseMoved(Coord mousePos) { return false; }
        public event Action<UIControl, Coord> MouseLeft;
        protected virtual bool OnMouseLeft(Coord mousePos) { IsMouseOver.V = false; return false; }
    }
}
