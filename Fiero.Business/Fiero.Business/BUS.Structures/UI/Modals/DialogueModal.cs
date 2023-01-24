using Fiero.Core;
using System;
using System.Linq;

namespace Fiero.Business
{
    public class DialogueModal : Modal
    {
        public readonly IDialogueTrigger Trigger;
        public readonly DialogueNode Node;
        public readonly DrawableEntity Speaker;
        public readonly DrawableEntity[] Listeners;

        public DialogueModal(
            GameUI ui,
            GameResources resources,
            IDialogueTrigger trigger,
            DialogueNode node,
            DrawableEntity speaker,
            params DrawableEntity[] listeners
        ) : base(ui, resources, new[] { ModalWindowButton.Ok }, ModalWindowStyles.None)
        {
            Trigger = trigger;
            Node = node;
            Speaker = speaker;
            Listeners = listeners;
        }

        public override void Open(string title)
        {
            Node.Trigger(Trigger, Speaker, Listeners);
            base.Open(title);
            if (Node.Choices.Count > 0)
            {
                var keys = Node.Choices.Keys.ToArray();
                if (Node.Cancellable)
                {
                    var modal = UI.OptionalChoice(keys, title);
                    modal.Cancelled += (_, btn) => Close(btn);
                    modal.OptionChosen += DialogueModal_OptionChosen;
                }
                else
                {
                    UI.NecessaryChoice(keys, title).OptionChosen += DialogueModal_OptionChosen;
                }
            }
        }

        private void DialogueModal_OptionChosen(ChoicePopUp<string> popUp, string option)
        {
            Close(ModalWindowButton.ImplicitYes);
            if (Node.Choices.TryGetValue(option, out var next) && next != null)
            {
                UI.Dialogue(Trigger, next, Speaker, Listeners);
            }
        }

        protected override void OnWindowSizeChanged(GameDatumChangedEventArgs<Coord> obj)
        {
            var popupSize = UI.Store.Get(Data.UI.PopUpSize);
            Layout.Size.V = new(popupSize.X, popupSize.Y / 2);
            Layout.Position.V = new(obj.NewValue.X / 2 - Layout.Size.V.X / 2, 0);
        }

        protected override void BeforePresentation()
        {
            var windowSize = UI.Store.Get(Data.UI.WindowSize);
            OnWindowSizeChanged(new(Data.UI.WindowSize, windowSize, windowSize));
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<UIControl>(s => s
                .Apply(x =>
                {
                    x.Background.V = Resources.Colors.Get(ColorName.Yellow);
                })
            )
            .AddRule<Picture>(s => s
                .Match(x => x.HasClass("portrait"))
                .Apply(x =>
                {
                    x.HorizontalAlignment.V = HorizontalAlignment.Center;
                    x.Sprite.V = Resources.Sprites.Get(TextureName.UI, $"face-{Node.Face}", ColorName.White);
                    x.LockAspectRatio.V = true;
                    x.Scale.V = new Vec(0.5f, 0.5f);
                }))
            .AddRule<Paragraph>(s => s
                .Match(x => x.HasClass("content"))
                .Apply(x =>
                {
                    x.Rows.V = 5;
                    x.Text.V = String.Join('\n', Node.Lines);
                    x.ContentAwareScale.V = false;
                }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid layout) => base.RenderContent(layout)
            .Row()
                .Col(@class: "portrait", w: 0.25f)
                    .Cell<Picture>()
                .End()
                .Col(@class: "content", w: 1.75f)
                    .Cell<Paragraph>()
                .End()
            .End()
            ;
    }
}
