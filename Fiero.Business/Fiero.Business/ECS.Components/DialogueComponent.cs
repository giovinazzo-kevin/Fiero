namespace Fiero.Business
{

    public class DialogueComponent : EcsComponent
    {
        public List<IDialogueTrigger> Triggers { get; set; } = new();
    }
}
