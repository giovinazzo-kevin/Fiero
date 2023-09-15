using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class DialogueModal : Modal
    {
        public readonly IDialogueTrigger Trigger;
        public readonly DialogueNode Node;
        public readonly DrawableEntity Speaker;
        public readonly DrawableEntity[] Listeners;
        protected readonly ChoicePopUp<string> Choices;

        static ModalWindowButton[] GetOptions(bool canCancel)
        {
            return Inner(canCancel).ToArray();
            IEnumerable<ModalWindowButton> Inner(bool canCancel)
            {
                yield return ModalWindowButton.Ok;
                if (canCancel) yield return ModalWindowButton.Cancel;
            }
        }

        public DialogueModal(
            GameUI ui,
            GameResources resources,
            IDialogueTrigger trigger,
            DialogueNode node,
            DrawableEntity speaker,
            params DrawableEntity[] listeners
        ) : base(ui, resources, GetOptions(node.Cancellable), GetDefaultStyles(GetOptions(node.Cancellable)))
        {
            Trigger = trigger;
            Node = node;
            Speaker = speaker;
            Listeners = listeners;
            IsResponsive = false;
            Choices = new ChoicePopUp<string>(UI, Resources, Node.Choices.Keys.ToArray(), Array.Empty<ModalWindowButton>());
            Choices.Cancelled += (_, btn) => Close(btn);
            Choices.OptionChosen += DialogueModal_OptionChosen;
        }
        protected override void OnLayoutRebuilt(Layout oldValue)
        {
            //base.OnLayoutRebuilt(oldValue);
            Layout.Size.V = UI.Store.Get(Data.UI.PopUpSize);
            Layout.Position.V = Layout.Size.V / 2 * new Coord(1, 0);
        }

        public override void Open(string title)
        {
            Node.Trigger(Trigger, Speaker, Listeners);
            base.Open(title);
        }

        private void DialogueModal_OptionChosen(ChoicePopUp<string> popUp, string option)
        {
            Close(ModalWindowButton.ImplicitYes);
            if (Node.Choices.TryGetValue(option, out var next) && next != null)
            {
                if (string.IsNullOrEmpty(next.Title))
                    next.Title = Node.Title;
                var dialogue = UI.Dialogue(Trigger, next, Speaker, Listeners);
                dialogue.Layout.Position.V = Layout.Position.V;
            }
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<Picture>(s => s
                .Match(x => x.HasClass("portrait"))
                .Apply(x =>
                {
                    x.Padding.V = new(8, 8);
                    x.HorizontalAlignment.V = HorizontalAlignment.Center;
                    x.VerticalAlignment.V = VerticalAlignment.Middle;
                    x.Sprite.V = Resources.Sprites.Get(TextureName.UI, $"face-{Node.Face}", ColorName.White);
                    x.LockAspectRatio.V = true;
                    x.Background.V = UI.GetColor(ColorName.UISecondary);
                }))
            .AddRule<Paragraph>(s => s
                .Match(x => x.HasClass("content"))
                .Apply(x =>
                {
                    x.Padding.V = new(8, 0);
                    x.Text.V = String.Join('\n', Node.Lines);
                    x.ContentAwareScale.V = false;
                    x.CenterContentV.V = true;
                    x.Background.V = UI.GetColor(ColorName.UISecondary);
                }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid layout) => base.RenderContent(layout)
            .Row(h: 80, px: true)
                .Col(@class: "portrait", w: 64 + 16, px: true)
                    .Cell<Picture>()
                .End()
                .Col(@class: "content")
                    .Cell<Paragraph>()
                .End()
            .End()
            .Row()
                .Cell<UIWindowAsControl>(w => w.Window.V = Choices)
            .End()
            ;
    }
}
