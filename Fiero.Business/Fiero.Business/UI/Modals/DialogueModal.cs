using Fiero.Core;
using SFML.Graphics;
using System;
using System.Linq;

namespace Fiero.Business
{
    public class DialogueModal : Modal
    {
        public readonly IDialogueTrigger Trigger;
        public readonly DialogueNode Node;
        public readonly Drawable Speaker;
        public readonly Drawable[] Listeners;

        public DialogueModal(
            GameUI ui, 
            IDialogueTrigger trigger, 
            DialogueNode node,
            Drawable speaker,
            params Drawable[] listeners
        ) : base(ui)
        {
            Trigger = trigger;
            Node = node;
            Speaker = speaker;
            Listeners = listeners;
        }

        public override void Open(string title, ModalWindowButtons buttons, ModalWindowStyles styles = default)
        {
            Node.Trigger(Trigger, Speaker, Listeners);
            base.Open(title, buttons, styles);
            if (Node.Choices.Count > 0) {
                UI.NecessaryChoice(Node.Choices.Keys.ToArray())
                    .OptionChosen += (w, opt) => {
                        Close(ModalWindowButtons.ImplicitYes);
                        if(Node.Choices.TryGetValue(opt, out var next)) {
                            UI.Dialogue(Trigger, next, Speaker, Listeners);
                        }
                    };
            }
        }

        protected override LayoutStyleBuilder DefineStyles(LayoutStyleBuilder builder) => base.DefineStyles(builder)
            .AddRule<UIControl>(s => s
                .Apply(x => {
                })
            )
            .AddRule<Picture<TextureName>>(s => s
                .Match(x => x.HasClass("portrait"))
                .Apply(x => {
                    x.HorizontalAlignment.V = HorizontalAlignment.Center;
                    x.TextureName.V = TextureName.UI;
                    x.LockAspectRatio.V = true;
                    x.SpriteName.V = $"face-{Node.Face}";
                    x.Scale.V = new Vec(0.5f, 0.5f);
                }))
            .AddRule<Paragraph>(s => s
                .Match(x => x.HasClass("content"))
                .Apply(x => {
                    x.MaxLines.V = 5;
                    x.Text.V = String.Join('\n', Node.Lines);
                }))
            ;

        protected override LayoutGrid RenderContent(LayoutGrid layout) => base.RenderContent(layout)
            .Row(h: 0.25f)
                .Col(@class: "portrait", w: 0.25f)
                    .Cell<Picture<TextureName>>()
                .End()
                .Col(@class: "content", w: 1.75f)
                    .Cell<Paragraph>()
                .End()
            .End()
            ;
    }
}
