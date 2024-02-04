namespace Fiero.Business
{

    public abstract class DialogueTrigger : IDialogueTrigger
    {
        private readonly string[] dialogueOptions;

        public readonly MetaSystem Systems;
        public string DialogueNode { get; private set; }
        string IDialogueTrigger.Node => DialogueNode.ToString();
        public bool Repeatable { get; protected set; }
        IList<object> IDialogueTrigger.Arguments => Arguments;
        public object[] Arguments { get; set; } = [];

        public event Action<DialogueTrigger, DialogueNode> Triggered;

        public DialogueTrigger(MetaSystem systems, bool repeatable, params string[] nodeChoices)
        {
            Systems = systems;
            dialogueOptions = nodeChoices;
            DialogueNode = Rng.Random.Choose(dialogueOptions);
            Repeatable = repeatable;
        }

        public abstract bool TryTrigger(FloorId floorId, PhysicalEntity speaker, out IEnumerable<DrawableEntity> listeners);
        public virtual void OnTrigger(DialogueNode node)
        {
            Triggered?.Invoke(this, node);
            DialogueNode = Rng.Random.Choose(dialogueOptions);
        }
    }
}
