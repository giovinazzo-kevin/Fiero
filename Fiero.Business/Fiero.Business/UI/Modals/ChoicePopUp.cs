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

        public ChoicePopUp(GameUI ui, params T[] options) : base(ui)
        {
            Options = options;
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<Button>(s => s
                .Match(x => x.HasClass("choice"))
                .Apply(x => {
                    x.FontSize.V = 16;
                }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid layout) => base.RenderContent(layout)
            .Col()
                .Repeat(Options.Length, (i, layout) => layout
                .Row(@class: "choice")
                    .Cell<Button>(b => {
                        b.Text.V = Options[i]?.ToString() ?? "(ERROR)";
                        b.Clicked += (_, __, ___) => {
                            if(SelectedIndex == i) {
                                Close(ModalWindowButtons.ImplicitYes);
                                OptionChosen?.Invoke(this, Options[i]);
                                return false;
                            }
                            SelectedIndex = i;
                            OptionClicked?.Invoke(this, Options[i]);
                            Invalidate();
                            return false;
                        };
                        Invalidated += () => {
                            b.Background.V = SelectedIndex == i
                                ? UI.Store.Get(Data.UI.DefaultAccent)
                                : UI.Store.Get(Data.UI.DefaultBackground);
                        };
                    })
                .End())
            .End()
            ;
    }
}
