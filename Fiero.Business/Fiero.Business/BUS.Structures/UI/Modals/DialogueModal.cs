namespace Fiero.Business
{
    public class DialogueModal : Modal
    {
        public readonly IDialogueTrigger Trigger;
        public readonly DialogueNode Node;
        public readonly DrawableEntity Speaker;
        public readonly DrawableEntity[] Listeners;
        protected readonly Actor[] Actors;

        protected readonly ChoicePopUp<string> Choices;

        protected const int DialogueHeight = 80;
        protected int ChoicesHeight;

        public event Action<DialogueModal, DialogueNode> NextChoice;

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
            Actors = Listeners
                .AsEnumerable()
                .Append(Speaker)
                .TrySelect(x => x.TryCast<Actor>(out var a) ? (true, a) : (false, default))
                .ToArray();
            IsResponsive = false;
            ModalWindowStyles? choicesStyle = Node.Choices.Count == 0 ? ModalWindowStyles.None : null;
            Choices = new ChoicePopUp<string>(UI, Resources, Node.Choices.Keys.ToArray(), Array.Empty<ModalWindowButton>(), styles: choicesStyle);
            Choices.Cancelled += (_, btn) => Close(btn);
            Choices.OptionChosen += DialogueModal_OptionChosen;
            Choices.Open(string.Empty);
            ChoicesHeight = Choices.Layout.Size.V.Y;
        }

        public override void Maximize()
        {
            Layout.Size.V = UI.Store.Get(Data.View.ViewportSize);
            Layout.Position.V = Coord.Zero;
            IsMaximized = true;
        }

        public override void Minimize()
        {
            Layout.Size.V = UI.Store.Get(Data.View.PopUpSize) * Coord.PositiveX
                + new Coord(0, TitleHeight + DialogueHeight + ChoicesHeight + ButtonsHeight);
            Layout.Position.V = Layout.Size.V / 2 * new Coord(1, 0);
            IsMaximized = false;
        }

        public override void Open(string title)
        {
            Node.Trigger(Trigger, Speaker, Listeners);
            foreach (var (line, actor) in Node.Lines.SelectMany(l => Actors.Select(a => (l, a))))
                actor.Log?.Write($"{Speaker.Info.Name}: {line}");
            base.Open(title);
        }

        private void DialogueModal_OptionChosen(ChoicePopUp<string> popUp, string option)
        {
            Close(ModalWindowButton.ImplicitYes);
            var player = Actors.Single(a => a.IsPlayer());
            foreach (var actor in Actors)
                actor.Log?.Write($"{player.Info.Name}: {option}");
            if (Node.Choices.TryGetValue(option, out var next) && next != null)
            {
                if (string.IsNullOrEmpty(next.Title))
                    next.Title = Node.Title;
                var dialogue = UI.Dialogue(Trigger, next, Speaker, Listeners);
                dialogue.Layout.Position.V = Layout.Position.V;
                dialogue.NextChoice += NextChoice;
                NextChoice?.Invoke(this, next);
            }
        }

        protected override LayoutThemeBuilder DefineStyles(LayoutThemeBuilder builder) => base.DefineStyles(builder)
            .Rule<Picture>(s => s
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
            .Rule<Paragraph>(s => s
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
            .Row(h: DialogueHeight, px: true)
                .Col(@class: "portrait", w: DialogueHeight, px: true)
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
