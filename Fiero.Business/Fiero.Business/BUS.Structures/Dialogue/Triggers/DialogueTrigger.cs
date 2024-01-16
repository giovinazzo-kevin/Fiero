namespace Fiero.Business
{

    public abstract class DialogueTrigger<TDialogue> : IDialogueTrigger
        where TDialogue : struct, Enum
    {
        private readonly TDialogue[] dialogueOptions;

        public readonly MetaSystem Systems;
        public TDialogue DialogueNode { get; private set; }
        private readonly string partialPath;
        string IDialogueTrigger.Node => DialogueNode.ToString();
        string IDialogueTrigger.FullPath => $"{partialPath}.{((IDialogueTrigger)this).Node}";
        public bool Repeatable { get; protected set; }

        public event Action<DialogueTrigger<TDialogue>> Triggered;

        public DialogueTrigger(MetaSystem systems, bool repeatable, string path, params TDialogue[] nodeChoices)
        {
            Systems = systems;
            partialPath = path;
            dialogueOptions = nodeChoices;
            DialogueNode = Rng.Random.Choose(dialogueOptions);
            Repeatable = repeatable;
        }

        public abstract bool TryTrigger(FloorId floorId, PhysicalEntity speaker, out IEnumerable<DrawableEntity> listeners);
        public virtual void OnTrigger()
        {
            Triggered?.Invoke(this);
            DialogueNode = Rng.Random.Choose(dialogueOptions);
        }
    }
}
