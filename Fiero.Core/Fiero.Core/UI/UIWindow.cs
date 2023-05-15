using System;

namespace Fiero.Core
{
    public abstract class UIWindow
    {
        public readonly GameUI UI;

        public readonly GameDatum<Coord> GameWindowSize;

        public Layout Layout { get; private set; }
        public UIControlProperty<string> Title { get; private set; }
        public UIControlProperty<Coord> Size { get; private set; } = new(nameof(Size));
        public UIControlProperty<Coord> Position { get; private set; } = new(nameof(Position));

        public event Action<UIWindow, ModalWindowButton> Closed;
        public event Action Updated;

        public virtual void Open(string title)
        {
            if (Title == null && title != null)
            {
                Title = new(nameof(Title), title);
            }
            RebuildLayout();
        }

        protected virtual void RebuildLayout()
        {
            Layout = UI.CreateLayout()
                .Build(Size.V, grid => CreateLayout(grid, Title ?? "Untitled"));
            Layout.Position.V = Position.V;
            OnGameWindowSizeChanged(new(GameWindowSize, new(), UI.Store.Get(GameWindowSize)));
        }

        public abstract LayoutGrid CreateLayout(LayoutGrid grid, string title);

        public UIWindow(GameUI ui, GameDatum<Coord> gameWindowSize)
        {
            UI = ui;
            Size.V = ui.Store.Get(gameWindowSize);
            Size.ValueUpdated += Size_ValueUpdated;
            Position.ValueUpdated += Position_ValueUpdated;
            (GameWindowSize = gameWindowSize).ValueChanged += OnGameWindowSizeChanged;
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

        protected virtual void OnGameWindowSizeChanged(GameDatumChangedEventArgs<Coord> obj)
        {
            Size.V = obj.NewValue;
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
            GameWindowSize.ValueChanged -= OnGameWindowSizeChanged;
            Closed?.Invoke(this, buttonPressed);
        }

        public virtual void Update()
        {
            Layout.Update();
            Updated?.Invoke();
        }

        public virtual void Draw()
        {
            UI.Window.RenderWindow.Draw(Layout);
        }
    }
}
