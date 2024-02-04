namespace Fiero.Business
{
    public class ChoicePopUp<T> : PopUp
    {
        readonly record struct Mapping(VirtualKeys Key, string Display);

        protected int SelectedIndex;

        public readonly string Message;
        public readonly T[] Options;
        public T SelectedOption => Options[SelectedIndex];

        public event Action<ChoicePopUp<T>, T> OptionChosen;
        public event Action<ChoicePopUp<T>, T> OptionClicked;

        protected readonly LayoutRef<Paragraph> Paragraph = new();
        protected int ParagraphHeight => Message == null ? 0 : Paragraph.Control.MinimumContentSize.Y;

        private readonly Dictionary<int, Mapping> _mapping = new();

        public ChoicePopUp(GameUI ui, GameResources resources, T[] options, ModalWindowButton[] buttons, string text = null, ModalWindowStyles? styles = null)
            : base(ui, resources, buttons, styles ?? GetDefaultStyles(buttons) & ~ModalWindowStyles.TitleBar_Maximize)
        {
            Message = text;
            Options = options;
            Confirmed += (_, __) =>
            {
                if (Options.Length > 0)
                    OptionChosen?.Invoke(this, Options[SelectedIndex]);
            };

            _mapping[0] = new(VirtualKeys.N1, "1");
            _mapping[1] = new(VirtualKeys.N2, "2");
            _mapping[2] = new(VirtualKeys.N3, "3");
            _mapping[3] = new(VirtualKeys.N4, "4");
            _mapping[4] = new(VirtualKeys.N5, "5");
            _mapping[5] = new(VirtualKeys.N6, "6");
            _mapping[6] = new(VirtualKeys.N7, "7");
            _mapping[7] = new(VirtualKeys.N8, "8");
            _mapping[8] = new(VirtualKeys.N9, "9");
            _mapping[9] = new(VirtualKeys.N0, "0");
        }

        public bool IsMapped(VirtualKeys key) => _mapping.Values.Any(m => m.Key == key);
        public void Remap(int slot, VirtualKeys key)
        {
            _mapping[slot] = new(key, VkToDisplay(key));
            RebuildLayout();
        }

        private string VkToDisplay(VirtualKeys vk)
        {
            var str = vk.ToString();
            if ((int)vk >= (int)VirtualKeys.A && (int)vk <= (int)VirtualKeys.Z)
                return str.ToLower().Substring(0, 1);
            if ((int)vk >= (int)VirtualKeys.N0 && (int)vk <= (int)VirtualKeys.N9)
                return str.ToLower().Substring(1, 1);
            return str;
        }

        public override void Update(TimeSpan t, TimeSpan dt)
        {
            base.Update(t, dt);

            if (UI.Input.IsKeyPressed(UI.Store.Get(Data.Hotkeys.MoveN))
                || UI.Input.IsMouseWheelScrollingUp())
            {
                SelectedIndex = (SelectedIndex - 1).Mod(Options.Length.DefaultIfZero(1));
                Invalidate();
            }
            if (UI.Input.IsKeyPressed(UI.Store.Get(Data.Hotkeys.MoveS))
                || UI.Input.IsMouseWheelScrollingDown())
            {
                SelectedIndex = (SelectedIndex + 1).Mod(Options.Length.DefaultIfZero(1));
                Invalidate();
            }

            foreach (var (i, map) in _mapping)
            {
                if (SelectNumber(map.Key, i))
                    break;
            }

            bool SelectNumber(VirtualKeys key, int index)
            {
                if (index < 0 || index >= Options.Length)
                {
                    return false;
                }
                if (UI.Input.IsKeyPressed(key))
                {
                    SelectedIndex = index;
                    Close(ModalWindowButton.ImplicitYes);
                    return true;
                }
                return false;
            }
        }

        protected override LayoutThemeBuilder DefineStyles(LayoutThemeBuilder builder) => base.DefineStyles(builder)
            .Rule<Button>(s => s
                .Match(x => x.HasClass("choice"))
                .Apply(x =>
                {
                    x.FontSize.V = x.Font.V.Size;
                    x.Padding.V = new(8, 0);
                    x.HorizontalAlignment.V = HorizontalAlignment.Left;
                }))
            .Rule<Paragraph>(s => s
                .Match(x => x.HasClass("message"))
                .Apply(x =>
                {
                    x.Padding.V = new(8, 8);
                }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid layout) => base.RenderContent(layout)
            .Col()
            .If(Message != null, g => g
                .Row(@class: "message")
                    .Cell<Layout>()
                    .Cell(Paragraph, p => p.Text.V = Message)
                .End())
            .Repeat(Options.Length, (i, layout) => layout
                .Row(h: 24, px: true, @class: "choice")
                    .Cell<Button>(b =>
                    {
                        b.Text.V = $"{_mapping[i].Display}) " + Options[i]?.ToString() ?? "(ERROR)";
                        b.MouseEntered += (_, __) => b.Background.V = UI.GetColor(ColorName.UIAccent);
                        b.MouseLeft += (_, __) => b.Background.V = UI.GetColor(ColorName.UIBackground);
                        b.Clicked += (_, __, ___) =>
                        {
                            if (SelectedIndex == i)
                            {
                                Close(ModalWindowButton.ImplicitYes);
                                return false;
                            }
                            SelectedIndex = i;
                            OptionClicked?.Invoke(this, Options[i]);
                            Invalidate();
                            return false;
                        };
                    })
                .End())
            .End()
            ;

        public override void Minimize()
        {
            var vwSize = UI.Store.Get(Data.View.ViewportSize);
            var popupSize = UI.Store.Get(Data.View.PopUpSize);
            Layout.Size.V = new(popupSize.X, 24 * Options.Length + TitleHeight + ButtonsHeight + ParagraphHeight);
            Layout.Position.V = vwSize / 2 - Layout.Size.V / 2;
        }
    }
}
