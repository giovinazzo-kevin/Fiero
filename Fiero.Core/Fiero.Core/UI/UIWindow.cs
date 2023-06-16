namespace Fiero.Core
{
    public abstract class UIWindow
    {
        public readonly GameUI UI;

        public Layout Layout { get; private set; }
        public UIControlProperty<string> Title { get; private set; }
        public UIControlProperty<Coord> Size { get; private set; } = new(nameof(Size));
        public UIControlProperty<Coord> Position { get; private set; } = new(nameof(Position));

        public event Action<UIWindow, ModalWindowButton> Closed;
        public event Action Updated;

        public bool IsOpen { get; private set; }

        protected virtual void SetDefaultSize()
        {
            Size.V = UI.Window.Size;
        }

        public virtual void Open(string title)
        {
            IsOpen = true;
            if (Size.V == default)
                SetDefaultSize();
            if (Title == null && title != null)
            {
                Title = new(nameof(Title), title);
            }
            RebuildLayout();
        }

        protected virtual void RebuildLayout()
        {
            Layout?.Dispose();
            Layout = UI.CreateLayout()
                .Build(Size.V, grid => CreateLayout(grid, Title ?? "Untitled"));
            Layout.Position.V = Position.V;
            Layout.Size.V = Size.V;
        }

        public abstract LayoutGrid CreateLayout(LayoutGrid grid, string title);

        public UIWindow(GameUI ui)
        {
            UI = ui;
            Size.ValueUpdated += Size_ValueUpdated;
            Position.ValueUpdated += Position_ValueUpdated;
        }

        private void Position_ValueUpdated(UIControlProperty<Coord> arg1, Coord old)
        {
            if (Layout == null) return;
            Layout.Position.V = Position.V;
        }

        void Size_ValueUpdated(UIControlProperty<Coord> arg1, Coord old)
        {
            if (Layout == null) return;
            Layout.Size.V = Size.V;
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
            Updated?.Invoke();
        }

        public virtual void Draw()
        {
            if (!IsOpen) return;
            UI.Window.RenderWindow.Draw(Layout);
        }
    }
}
