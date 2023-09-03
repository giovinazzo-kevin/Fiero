using SFML.Graphics;

namespace Fiero.Core
{
    public abstract class ModalWindow : Widget
    {
        public event Action<ModalWindow, ModalWindowButton> Confirmed;
        public event Action<ModalWindow, ModalWindowButton> Cancelled;

        protected readonly ModalWindowButton[] Buttons;
        protected readonly ModalWindowStyles Styles;

        protected int TitleHeight, ButtonsHeight;

        public ModalWindow(GameUI ui, ModalWindowButton[] buttons, ModalWindowStyles styles = ModalWindowStyles.Default)
            : base(ui)
        {
            Buttons = buttons;
            Styles = styles;
        }

        public override LayoutGrid CreateLayout(LayoutGrid grid, string title)
        {
            Layout?.Dispose();
            var hasTitle = Styles.HasFlag(ModalWindowStyles.Title);
            var hasButtons = Styles.HasFlag(ModalWindowStyles.CustomButtons) && Buttons.Length > 0;
            var hasTitleBar = Styles.HasFlag(ModalWindowStyles.TitleBar);

            TitleHeight = hasTitle ? 16 : 0;
            ButtonsHeight = hasButtons ? 24 : 0;
            var contentHeight = 1f;
            return ApplyStyles(grid)
                .Col(@class: "modal")
                    .If(hasTitle, g => g
                        .Row(h: TitleHeight, px: true, @class: "modal-title")
                            .Col()
                                .Cell<Label>(l =>
                                {
                                    l.Text.V = title;
                                    l.HorizontalAlignment.V = HorizontalAlignment.Center;
                                    l.Foreground.V = Color.White;
                                    l.OutlineColor.V = Color.White;
                                    l.OutlineThickness.V = 1;
                                    l.ZOrder.V = -1;
                                    if (Title != null)
                                    {
                                        Title.V = l.Text.V;
                                    }
                                })
                            .End()
                            .If(hasTitleBar, g => g
                                .Col(w: 16, px: true, @class: "modal-title modal-close")
                                    .Cell<Button>(b =>
                                    {
                                        b.Text.V = "x";
                                        b.Foreground.V = Color.White;
                                        b.Background.V = Color.Red;
                                        b.OutlineColor.V = Color.White;
                                        b.OutlineThickness.V = 1;
                                        b.VerticalAlignment.V = VerticalAlignment.Middle;
                                        b.HorizontalAlignment.V = HorizontalAlignment.Center;
                                        b.ZOrder.V = -1;
                                        b.Clicked += (_, __, ___) =>
                                        {
                                            Close(new("modal-close", null));
                                            return false;
                                        };
                                    })
                            .End())
                        .End()
                    )
                    .Row(h: contentHeight, @class: "modal-content", id: "modal-content")
                        .Repeat(1, (i, g) => RenderContent(g))
                    .End()
                    .If(hasButtons, g => g.Row(h: ButtonsHeight, px: true, @class: "modal-controls")
                        .Repeat(Buttons.Length, (i, grid) => grid
                            .Col()
                                .Cell<Button>(b =>
                                {
                                    b.Text.V = Buttons[i].ToString();
                                    b.FontSize.V = new Coord(16, 24);
                                    b.HorizontalAlignment.V = HorizontalAlignment.Center;
                                    b.Clicked += (_, __, ___) =>
                                    {
                                        Close(Buttons[i]);
                                        return false;
                                    };
                                })
                            .End()
                        )
                    .End())
                .End();
        }

        public override void Close(ModalWindowButton buttonPressed)
        {
            base.Close(buttonPressed);
            // ResultType is nullable
            if (buttonPressed.ResultType == true)
            {
                Confirmed?.Invoke(this, buttonPressed);
            }
            else if (buttonPressed.ResultType == false)
            {
                Cancelled?.Invoke(this, buttonPressed);
            }
        }
    }
}
