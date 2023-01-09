using Fiero.Core;
using SFML.Graphics;
using SFML.Window;
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
            Confirmed += (_, __) => {
                OptionChosen?.Invoke(this, Options[SelectedIndex]);
            };
        }

        public override void Update()
        {
            base.Update();

            if (UI.Input.IsKeyPressed(UI.Store.Get(Data.Hotkeys.MoveN))) {
                SelectedIndex = (SelectedIndex - 1).Mod(Options.Length);
                Invalidate();
            }
            if (UI.Input.IsKeyPressed(UI.Store.Get(Data.Hotkeys.MoveS))) {
                SelectedIndex = (SelectedIndex + 1).Mod(Options.Length);
                Invalidate();
            }

            SelectNumber(Keyboard.Key.Num1, 0);
            SelectNumber(Keyboard.Key.Num2, 1);
            SelectNumber(Keyboard.Key.Num3, 2);
            SelectNumber(Keyboard.Key.Num4, 3);
            SelectNumber(Keyboard.Key.Num5, 4);
            SelectNumber(Keyboard.Key.Num6, 5);
            SelectNumber(Keyboard.Key.Num7, 6);
            SelectNumber(Keyboard.Key.Num8, 7);
            SelectNumber(Keyboard.Key.Num9, 8);

            void SelectNumber(Keyboard.Key key, int index)
            {
                if(index < 0 || index >= Options.Length) {
                    return;
                }
                if (UI.Input.IsKeyPressed(key)) {
                    if(SelectedIndex == index) {
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
                .Apply(x => {
                    x.FontSize.V = 16;
                    x.Background.V = SelectedIndex == x.ZOrder.V
                        ? UI.Store.Get(Data.UI.DefaultAccent)
                        : UI.Store.Get(Data.UI.DefaultBackground);
                }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid layout) => base.RenderContent(layout)
            .Col()
                .Repeat(Options.Length, (i, layout) => layout
                .Row(@class: "choice")
                    .Cell<Button>(b => {
                        b.ZOrder.V = i;
                        b.Text.V = $"{i+1}) " + Options[i]?.ToString() ?? "(ERROR)";
                        b.Clicked += (_, __, ___) => {
                            if(SelectedIndex == i) {
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
