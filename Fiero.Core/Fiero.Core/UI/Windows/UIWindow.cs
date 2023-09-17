using SFML.Graphics;

namespace Fiero.Core
{
    public abstract class UIWindow
    {
        public readonly GameUI UI;

        public Layout Layout { get; private set; }
        public UIControlProperty<string> Title { get; private set; }

        public event Action<UIWindow> Opened;
        public event Action<UIWindow, ModalWindowButton> Closed;
        public event Action<UIWindow> Updated;

        public bool IsOpen { get; private set; }


        protected abstract void DefaultSize();

        public virtual void Open(string title)
        {
            IsOpen = true;
            if (Title == null && title != null)
            {
                Title = new(nameof(Title), title);
            }
            RebuildLayout();
            DefaultSize();
            Opened?.Invoke(this);
        }

        protected virtual void OnLayoutRebuilt(Layout oldValue) { }

        protected virtual void RebuildLayout()
        {
            var oldLayout = Layout;
            var oldSize = oldLayout?.Size.V ?? Coord.Zero;
            var oldPos = oldLayout?.Position.V ?? Coord.Zero;
            Layout = UI.CreateLayout()
                .Build(UI.Window.Size, grid => CreateLayout(grid, Title ?? "Untitled"));
            Layout.Position.V = oldPos;
            Layout.Size.V = oldSize;
            OnLayoutRebuilt(oldLayout);
            oldLayout?.Dispose();
            Layout.Invalidate();
        }

        public virtual LayoutGrid CreateLayout(LayoutGrid grid, string title) => ApplyStyles(grid)
            .Col()
                .Repeat(1, (i, g) => RenderContent(g))
            .End();

        public UIWindow(GameUI ui)
        {
            UI = ui;
        }

        protected LayoutGrid ApplyStyles(LayoutGrid grid)
        {
            var styleBuilder = DefineStyles(new LayoutThemeBuilder(grid.Theme));
            var styles = styleBuilder.Build();
            foreach (var s in styles)
            {
                styles = styles.Style(s);
            }
            grid.Theme = styles;
            return grid;
        }

        protected virtual LayoutThemeBuilder DefineStyles(LayoutThemeBuilder builder)
        {
            return builder;
        }

        protected virtual LayoutGrid RenderContent(LayoutGrid layout)
        {
            return layout;
        }

        public virtual void Close(ModalWindowButton buttonPressed)
        {
            IsOpen = false;
            Closed?.Invoke(this, buttonPressed);
        }

        public virtual void Update(TimeSpan t, TimeSpan dt)
        {
            if (!IsOpen) return;
            Layout.Update(t, dt);
            Updated?.Invoke(this);
        }

        public virtual void Draw(RenderTarget target, RenderStates states)
        {
            if (!IsOpen) return;
            Layout.Draw(target, states);
        }
    }
}
