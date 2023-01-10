﻿using System;

namespace Fiero.Core
{
    public abstract class UIWindow
    {
        public readonly GameUI UI;

        public Layout Layout { get; private set; }
        public UIControlProperty<string> Title { get; private set; }

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
                .Build(new(), grid => CreateLayout(grid, Title ?? "Untitled"));
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
