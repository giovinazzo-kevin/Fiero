using Fiero.Core;
using SFML.Graphics;
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

        public ChoicePopUp(GameUI ui, params T[] options) : base(ui)
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
                        b.Text.V = Options[i]?.ToString() ?? "(ERROR)";
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
