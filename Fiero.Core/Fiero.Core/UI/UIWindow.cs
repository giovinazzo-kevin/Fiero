namespace Fiero.Core
{
    public abstract class UIWindow
    {
        public readonly GameUI UI;

        public Layout Layout { get; private set; }
        public UIControlProperty<string> Title { get; private set; }

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
        }

        public abstract LayoutGrid CreateLayout(LayoutGrid grid, string title);

        public UIWindow(GameUI ui)
        {
            UI = ui;
        }

        protected LayoutGrid ApplyStyles(LayoutGrid grid)
        {
            var styleBuilder = DefineStyles(new LayoutStyleBuilder());
            var styles = styleBuilder.Build();
            foreach (var s in styles)
            {
                grid = grid.Style(s);
            }
            return grid;
        }

        protected virtual LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder)
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

        public virtual void Update()
        {
            if (!IsOpen) return;
            Layout.Update();
            Updated?.Invoke(this);
        }

        public virtual void Draw()
        {
            if (!IsOpen) return;
            UI.Window.RenderWindow.Draw(Layout);
        }
    }
}
