using Ergo.Lang;

namespace Fiero.Business
{
    [Term(Marshalling = TermMarshalling.Named)]
    public class DialogueNode
    {
        public string Id { get; init; }
        public string Face { get; init; }
        public string Title { get; set; }
        public string[] Lines { get; init; }
        public bool Cancellable { get; init; }
        [NonTerm]
        public DialogueNode Next { get; set; }
        [NonTerm]
        public IDictionary<string, DialogueNode> Choices { get; init; }

        public event Action<IDialogueTrigger, DialogueTriggeredEventArgs> Triggered;

        public void Trigger(IDialogueTrigger trigger, DrawableEntity speaker, params DrawableEntity[] listeners)
        {
            Triggered?.Invoke(trigger, new(this, speaker, listeners));
        }

        public DialogueNode(
            string id,
            string face,
            string title,
            string[] lines,
            bool cancellable,
            DialogueNode next = null,
            IDictionary<string, DialogueNode> choices = null)
        {
            Id = id;
            Face = face;
            Lines = lines;
            Next = next;
            Choices = choices ?? new Dictionary<string, DialogueNode>();
            Cancellable = cancellable;
            Title = title;
        }
    }
}
