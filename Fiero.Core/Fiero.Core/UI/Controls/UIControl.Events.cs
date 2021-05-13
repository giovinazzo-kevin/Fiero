using System;

namespace Fiero.Core
{
    public partial class UIControl
    {
        public event Action<UIControl, Coord> Clicked;
        protected virtual void OnClicked(Coord mousePos) { }
        public event Action<UIControl, Coord> MouseEntered;
        protected virtual void OnMouseEntered(Coord mousePos) { IsMouseOver.V = true; }
        public event Action<UIControl, Coord> MouseMoved;
        protected virtual void OnMouseMoved(Coord mousePos) { }
        public event Action<UIControl, Coord> MouseLeft;
        protected virtual void OnMouseLeft(Coord mousePos) { IsMouseOver.V = false; }
    }
}
