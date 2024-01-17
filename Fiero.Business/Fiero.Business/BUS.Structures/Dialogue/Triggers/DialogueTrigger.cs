namespace Fiero.Business
{

    public abstract class DialogueTrigger : IDialogueTrigger
    {
        private readonly string[] dialogueOptions;

        public readonly MetaSystem Systems;
        public string DialogueNode { get; private set; }
        string IDialogueTrigger.Node => DialogueNode.ToString();
        public bool Repeatable { get; protected set; }

        public event Action<DialogueTrigger> Triggered;

        public DialogueTrigger(MetaSystem systems, bool repeatable, params string[] nodeChoices)
        {
            Systems = systems;
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
