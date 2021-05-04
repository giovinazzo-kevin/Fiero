using System;
using System.Collections;
using System.Collections.Generic;

namespace Fiero.Business
{

    public abstract class DialogueTrigger<TDialogue> : IDialogueTrigger
        where TDialogue : struct, Enum
    {
        public readonly TDialogue DialogueNode;
        string IDialogueTrigger.DialogueNode => DialogueNode.ToString();
        public bool Repeatable { get; protected set; }

        public DialogueTrigger(TDialogue node, bool repeatable)
        {
            DialogueNode = node;
            Repeatable = repeatable;
        }

        public abstract bool TryTrigger(Floor floor, Drawable speaker, out IEnumerable<Drawable> listeners);
    }
}
