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

        public DialogueNode Format(IList<object> args)
        {
            var newTitle = Title;
            var newLines = Lines.ToArray();
            var newChoices = Choices.ToArray();
            // Formattable fields: Title, Lines, Choices.Keys
            for (int i = 0; i < args.Count; i++)
            {
                var placeholder = $"{{{i}}}";
                var arg = $"{args[i]}";
                newTitle = newTitle.Replace(placeholder, arg);
                for (int l = 0; l < Lines.Length; l++)
                {
                    newLines[l] = newLines[l].Replace(placeholder, arg);
                }
                for (int c = 0; c < Choices.Count; c++)
                {
                    var n = newChoices[c];
                    newChoices[c] = new(n.Key.Replace(placeholder, arg), n.Value);
                }
            }
            return new DialogueNode(Id, Face, newTitle, newLines, Cancellable, Next, new Dictionary<string, DialogueNode>(newChoices));
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
