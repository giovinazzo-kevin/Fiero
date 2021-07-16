using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Core
{
    public abstract class ModalWindow
    {
        public readonly GameUI UI;

        public Layout Layout { get; private set; }
        public UIControlProperty<string> Title { get; private set; }

        public event Action<ModalWindow, ModalWindowButton> Closed;
        public event Action<ModalWindow, ModalWindowButton> Confirmed;
        public event Action<ModalWindow, ModalWindowButton> Cancelled;
        public event Action Updated;


        public virtual void Open(string title, ModalWindowButton[] buttons, ModalWindowStyles styles = ModalWindowStyles.Default)
        {
            Layout = UI.CreateLayout()
                .Build(new(), grid => CreateLayout(grid, title, buttons, styles));
        }

        public virtual LayoutGrid CreateLayout(LayoutGrid grid, string title, ModalWindowButton[] buttons, ModalWindowStyles styles = ModalWindowStyles.Default)
        {
            var hasTitle = styles.HasFlag(ModalWindowStyles.Title);
            var hasButtons = styles.HasFlag(ModalWindowStyles.Buttons);

            var titleHeight =  hasTitle ? 0.20f : 0f;
            var buttonsHeight = hasButtons ? 0.20f : 0f;
            var contentHeight = hasTitle && hasButtons ? 2.60f : hasTitle ^ hasButtons ? 1.80f : 1f;

            return ApplyStyles(grid)
                .Col(@class: "modal")
                    .If(hasTitle, g => g.Row(h: titleHeight, @class: "modal-title")
                        .Cell<Label>(l => {
                            l.Text.V = title;
                            l.CenterContentH.V = true;
                            Title = l.Text;
                        })
                    .End())
                    .Row(h: contentHeight, @class: "modal-content")
                        .Repeat(1, (i, g) => RenderContent(g))
                    .End()
                    .If(hasButtons, g => g.Row(h: buttonsHeight, @class: "modal-controls")
                        .Repeat(buttons.Length, (i, grid) => grid
                            .Col()
                                .Cell<Button>(b => {
                                    b.Text.V = buttons[i].ToString();
                                    b.CenterContentH.V = true;
                                    b.Clicked += (_, __, ___) => {
                                        Close(buttons[i]);
                                        return false;
                                    };
                                })
                            .End()
                        )
                    .End())
                .End();
        }

        public ModalWindow(GameUI ui) 
        {
            UI = ui;
        }

        private LayoutGrid ApplyStyles(LayoutGrid grid)
        {
            var styleBuilder = DefineStyles(new LayoutStyleBuilder());
            var styles = styleBuilder.Build();
            foreach (var s in styles) {
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
            // ResultType is nullable
            if (buttonPressed.ResultType == true) {
                Confirmed?.Invoke(this, buttonPressed);
            }
            else if (buttonPressed.ResultType == false) {
                Cancelled?.Invoke(this, buttonPressed);
            }
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
