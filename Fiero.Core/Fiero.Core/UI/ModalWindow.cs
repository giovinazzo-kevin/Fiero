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

        public event Action<ModalWindow, ModalWindowButtons> Closed;
        public event Action<ModalWindow, ModalWindowButtons> Confirmed;
        public event Action<ModalWindow, ModalWindowButtons> Cancelled;
        public event Action<float, float> Updated;


        public virtual void Open(string title, ModalWindowButtons buttons, ModalWindowStyles styles = ModalWindowStyles.Default)
        {
            Layout = UI.CreateLayout()
                .Build(new(), grid => CreateLayout(grid, title, buttons, styles));
        }

        public virtual LayoutGrid CreateLayout(LayoutGrid grid, string title, ModalWindowButtons buttons, ModalWindowStyles styles = ModalWindowStyles.Default)
        {
            var enabledButtons = Enum.GetValues<ModalWindowButtons>()
                .Skip(1)
                .Where(v => buttons.HasFlag(v))
                .ToArray();

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
                        .Repeat(enabledButtons.Length, (i, grid) => grid
                            .Col()
                                .Cell<Button>(b => {
                                    b.Text.V = enabledButtons[i].ToString();
                                    b.CenterContentH.V = true;
                                    b.Clicked += (_, __, ___) => {
                                        Close(enabledButtons[i]);
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

        public virtual void Close(ModalWindowButtons buttonPressed)
        {
            Closed?.Invoke(this, buttonPressed);
            if (IsResultPositive(buttonPressed)) {
                Confirmed?.Invoke(this, buttonPressed);
            }
            if (IsResultNegative(buttonPressed)) {
                Cancelled?.Invoke(this, buttonPressed);
            }
        }

        public virtual void Update(RenderWindow win, float t, float dt)
        {
            Layout.Update(t, dt);
            Updated?.Invoke(t, dt);
        }

        public virtual void Draw(RenderWindow win, float t, float dt)
        {
            win.Draw(Layout);
        }

        public static bool IsResultPositive(ModalWindowButtons buttonPressed) =>
            buttonPressed == ModalWindowButtons.Yes
                || buttonPressed == ModalWindowButtons.ImplicitYes
                || buttonPressed == ModalWindowButtons.Ok;
        public static bool IsResultNegative(ModalWindowButtons buttonPressed) =>
            buttonPressed == ModalWindowButtons.No
                || buttonPressed == ModalWindowButtons.ImplicitNo
                || buttonPressed == ModalWindowButtons.Cancel;
    }
}
