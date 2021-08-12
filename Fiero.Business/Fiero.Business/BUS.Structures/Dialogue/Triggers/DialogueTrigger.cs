using Fiero.Core;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Fiero.Business
{

    public abstract class DialogueTrigger<TDialogue> : IDialogueTrigger
        where TDialogue : struct, Enum
    {
        private readonly TDialogue[] _dialogueOptions;

        public readonly GameSystems Systems;
        public TDialogue DialogueNode { get; private set; }
        string IDialogueTrigger.DialogueNode => DialogueNode.ToString();
        public bool Repeatable { get; protected set; }

        public event Action<DialogueTrigger<TDialogue>> Triggered;

        public DialogueTrigger(GameSystems systems, bool repeatable, params TDialogue[] nodeChoices)
        {
            Systems = systems;
            _dialogueOptions = nodeChoices;
            DialogueNode = Rng.Random.Choose(_dialogueOptions);
            Repeatable = repeatable;
        }

        public abstract bool TryTrigger(FloorId floorId, PhysicalEntity speaker, out IEnumerable<DrawableEntity> listeners);
        public virtual void OnTrigger()
        {
            Triggered?.Invoke(this);
            DialogueNode = Rng.Random.Choose(_dialogueOptions);
        }
    }
}
