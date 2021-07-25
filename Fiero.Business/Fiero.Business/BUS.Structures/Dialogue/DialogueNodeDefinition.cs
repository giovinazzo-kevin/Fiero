namespace Fiero.Business
{
    public readonly struct DialogueNodeDefinition
    {
        public readonly string Id;
        public readonly string Face;
        public readonly string[] Lines;
        public readonly bool Cancellable;
        public readonly (string Line, string Next)[] Choices;
        public readonly string Next;
        public DialogueNodeDefinition(string id, string face, string[] lines, bool cancellable, (string, string)[] choices, string next)
        {
            Id = id;
            Face = face;
            Lines = lines;
            Cancellable = cancellable;
            Choices = choices;
            Next = next;
        }
    }
}
