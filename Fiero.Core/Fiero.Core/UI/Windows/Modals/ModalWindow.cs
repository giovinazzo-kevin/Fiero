namespace Fiero.Core
{
    public abstract class ModalWindow : Widget
    {
        public event Action<ModalWindow, ModalWindowButton> Confirmed;
        public event Action<ModalWindow, ModalWindowButton> Cancelled;

        protected readonly ModalWindowButton[] Buttons;
        protected readonly ModalWindowStyles Styles;

        protected int TitleHeight, ButtonsHeight;
        protected abstract bool IsMaximized { get; set; }

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
            var hasCloseButton = Styles.HasFlag(ModalWindowStyles.TitleBar_Close);
            var hasMaximizeButton = Styles.HasFlag(ModalWindowStyles.TitleBar_Maximize);

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
                                    l.OutlineThickness.V = 1;
                                    l.ZOrder.V = -1;
                                    if (Title != null)
                                    {
                                        Title.V = l.Text.V;
                                    }
                                })
                            .End()
                            .If(hasMaximizeButton, g => g
                                .Col(w: 16, px: true, @class: "modal-title modal-maximize")
                                    .Cell<Button>(b =>
                                    {
                                        b.Text.V = "O";
                                        b.OutlineThickness.V = 1;
                                        b.VerticalAlignment.V = VerticalAlignment.Middle;
                                        b.HorizontalAlignment.V = HorizontalAlignment.Center;
                                        b.ZOrder.V = -1;
                                        b.Clicked += B_Clicked;
                                        void B_Clicked(UIControl arg1, Coord arg2, SFML.Window.Mouse.Button arg3)
                                        {
                                            if (!IsMaximized)
                                            {
                                                b.Text.V = "o";
                                                Maximize();
                                            }
                                            else
                                            {
                                                b.Text.V = "O";
                                                Minimize();
                                            }
                                        }
                                    })
                            .End())
                            .If(hasCloseButton, g => g
                                .Col(w: 16, px: true, @class: "modal-title modal-close")
                                    .Cell<Button>(b =>
                                    {
                                        b.Text.V = "x";
                                        b.OutlineThickness.V = 1;
                                        b.VerticalAlignment.V = VerticalAlignment.Middle;
                                        b.HorizontalAlignment.V = HorizontalAlignment.Center;
                                        b.ZOrder.V = -1;
                                        b.Clicked += (_, __, ___) =>
                                        {
                                            Close(new("modal-close", null));
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
                                    };
                                })
                            .End()
                        )
                    .End())
                .End();
        }

        protected override void DefaultSize()
        {
            Minimize();
        }
        public abstract void Maximize();
        public abstract void Minimize();

        public Task<ModalWindowButton> WaitForClose()
        {
            var tcs = new TaskCompletionSource<ModalWindowButton>();
            Closed += SetResult;
            return tcs.Task;
            void SetResult(UIWindow win, ModalWindowButton btn)
            {
                tcs.SetResult(btn);
                Closed -= SetResult;
            }
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
