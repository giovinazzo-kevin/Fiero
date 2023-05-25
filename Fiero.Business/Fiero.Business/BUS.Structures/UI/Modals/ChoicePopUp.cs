using Fiero.Core;
using System;

namespace Fiero.Business
{
    public class ChoicePopUp<T> : PopUp
    {
        protected int SelectedIndex;

        public readonly T[] Options;
        public T SelectedOption => Options[SelectedIndex];

        public event Action<ChoicePopUp<T>, T> OptionChosen;
        public event Action<ChoicePopUp<T>, T> OptionClicked;

        public ChoicePopUp(GameUI ui, GameResources resources, T[] options, ModalWindowButton[] buttons, ModalWindowStyles styles = ModalWindowStyles.Default)
            : base(ui, resources, buttons, styles)
        {
            Options = options;
            Confirmed += (_, __) =>
            {
                OptionChosen?.Invoke(this, Options[SelectedIndex]);
            };
            Data.UI.WindowSize.ValueChanged += e =>
            {
                SetDefaultSize();
            };
        }

        protected override void SetDefaultSize()
        {
            var windowSize = UI.Store.Get(Data.UI.WindowSize);
            Size.V = new(windowSize.X, 200);
            Position.V = new(0, windowSize.Y - 200);
        }

        public override void Update()
        {
            base.Update();

            if (UI.Input.IsKeyPressed(UI.Store.Get(Data.Hotkeys.MoveN)))
            {
                SelectedIndex = (SelectedIndex - 1).Mod(Options.Length);
                Invalidate();
            }
            if (UI.Input.IsKeyPressed(UI.Store.Get(Data.Hotkeys.MoveS)))
            {
                SelectedIndex = (SelectedIndex + 1).Mod(Options.Length);
                Invalidate();
            }

            SelectNumber(VirtualKeys.N1, 0);
            SelectNumber(VirtualKeys.N2, 1);
            SelectNumber(VirtualKeys.N3, 2);
            SelectNumber(VirtualKeys.N4, 3);
            SelectNumber(VirtualKeys.N5, 4);
            SelectNumber(VirtualKeys.N6, 5);
            SelectNumber(VirtualKeys.N7, 6);
            SelectNumber(VirtualKeys.N8, 7);
            SelectNumber(VirtualKeys.N9, 8);

            void SelectNumber(VirtualKeys key, int index)
            {
                if (index < 0 || index >= Options.Length)
                {
                    return;
                }
                if (UI.Input.IsKeyPressed(key))
                {
                    if (SelectedIndex == index)
                    {
                        Close(ModalWindowButton.ImplicitYes);
                        return;
                    }
                    SelectedIndex = index;
                    Invalidate();
                }
            }
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<Button>(s => s
                .Match(x => x.HasClass("choice"))
                .Apply(x =>
                {
                    x.FontSize.V = x.Font.V.Size;
                    x.Padding.V = new(8, 0);
                    x.HorizontalAlignment.V = HorizontalAlignment.Left;
                    x.Background.V = SelectedIndex == x.ZOrder.V
                        ? UI.Store.Get(Data.UI.DefaultAccent)
                        : UI.Store.Get(Data.UI.DefaultBackground);
                }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid layout) => base.RenderContent(layout)
            .Col()
                .Repeat(Options.Length, (i, layout) => layout
                .Row(@class: "choice")
                    .Cell<Button>(b =>
                    {
                        b.ZOrder.V = i;
                        b.Text.V = $"{i + 1}) " + Options[i]?.ToString() ?? "(ERROR)";
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
                .Repeat(Math.Max(0, 4 - Options.Length), (i, layout) => layout
                .Row(@class: "spacer")
                    .Cell<Layout>(x => x.Background.V = UI.Store.Get(Data.UI.DefaultBackground))
                .End())
            .End()
            ;
    }
}
