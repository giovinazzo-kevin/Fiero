namespace Fiero.Core
{
    public class Layout : UIControl
    {
        public readonly LayoutGrid Dom;

        public bool MediaQuery(Func<Coord, bool> match)
        {
            return match(Size.V);
        }

        public IEnumerable<UIControl> Query(Func<Layout, bool> match, Func<LayoutGrid, bool> select)
        {
            if (!match(this))
                return Enumerable.Empty<UIControl>();
            return Dom.Query(select).SelectMany(x => x.Controls.Select(x => x.Instance));
        }

        public IEnumerable<T> Query<T>(Func<Layout, bool> match, Func<LayoutGrid, bool> select = null)
            where T : UIControl
            => Query(match, x => x.Is<T>() && (select?.Invoke(x) ?? true)).OfType<T>();

        public virtual void Focus(UIControl control)
        {
            foreach (var c in Children)
            {
                c.IsActive.V = false;
            }
            if (control != this && control.IsInteractive)
            {
                control.IsActive.V = true;
            }
        }

        public override void Update()
        {
            var preventDefault = TrackMouse(Input.GetMousePosition(), out var clickedControl, out var clickedButton);
            if (!preventDefault)
            {
                if (clickedControl != null && !clickedControl.IsHidden.V && clickedControl.IsInteractive.V)
                {
                    Focus(clickedControl);
                }
            }
            base.Update();
        }

        public Layout(LayoutGrid dom, GameInput input, params UIControl[] controls) : base(input)
        {
            Dom = dom;
            Children.AddRange(controls);
        }

        public override void Dispose()
        {
            foreach (var item in Children)
            {
                item.Dispose();
            }
        }
    }
}
