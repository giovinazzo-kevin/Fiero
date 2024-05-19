using SFML.Window;

namespace Fiero.Core
{
    public partial class UIControl
    {
        public event Action<UIControl, Coord, Mouse.Button> Clicked;
        protected virtual bool OnClicked(Coord mousePos, Mouse.Button button) { return false; }
        public event Action<UIControl, Coord> MouseEntered;
        protected virtual bool OnMouseEntered(Coord mousePos) { IsMouseOver = true; return false; }
        public event Action<UIControl, Coord> MouseMoved;
        protected virtual bool OnMouseMoved(Coord mousePos) { return false; }
        public event Action<UIControl, Coord> MouseLeft;
        protected virtual bool OnMouseLeft(Coord mousePos) { IsMouseOver = false; return false; }

        public void MouseOver(Action<UIControl, Coord> handler)
        {
            var restore = new HashSet<Action<UIControl, Coord>>();
            handler = ((_, __) =>
            {
                PropertyChanging += OnPropertyChanging;
            }) + handler;
            MouseEntered += handler;
            MouseLeft += Restore;
            void OnPropertyChanging(object sender, System.ComponentModel.PropertyChangingEventArgs e)
            {
                var prop = Properties.Single(p => p.Name.Equals(e.PropertyName));
                var value = prop.Value;
                restore.Add((_, __) => prop.Value = value);
            }
            void Restore(UIControl arg1, Coord arg2)
            {
                PropertyChanging -= OnPropertyChanging;
                foreach (var x in restore)
                    x(arg1, arg2);
            }
        }
        public void MouseOver(Action<UIControl, Coord> mouseEnter, Action<UIControl, Coord> mouseLeft)
        {
            MouseEntered += mouseEnter;
            MouseLeft += mouseLeft;
        }
    }
}
