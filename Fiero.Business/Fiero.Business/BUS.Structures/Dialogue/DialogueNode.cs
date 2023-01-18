using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class DialogueNode
    {
        public readonly string Id;
        public readonly string Face;
        public readonly string Title;
        public readonly string[] Lines;
        public readonly bool Cancellable;
        public readonly IDictionary<string, DialogueNode> Choices;
        public DialogueNode Next { get; set; }

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
