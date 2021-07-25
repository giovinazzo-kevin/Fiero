using System;
using System.Collections;
using System.Collections.Generic;

namespace Fiero.Business
{

    public abstract class DialogueTrigger<TDialogue> : IDialogueTrigger
        where TDialogue : struct, Enum
    {
        public readonly GameSystems Systems;
        public readonly TDialogue DialogueNode;
        string IDialogueTrigger.DialogueNode => DialogueNode.ToString();
        public bool Repeatable { get; protected set; }

        public event Action<DialogueTrigger<TDialogue>> Triggered;

        public DialogueTrigger(GameSystems systems, TDialogue node, bool repeatable)
        {
            Systems = systems;
            DialogueNode = node;
            Repeatable = repeatable;
        }

        public abstract bool TryTrigger(FloorId floorId, Drawable speaker, out IEnumerable<Drawable> listeners);
        public virtual void OnTrigger()
        {
            Triggered?.Invoke(this);
        }
    }
}
