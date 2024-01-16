namespace Fiero.Business
{
    public class DialogueEntity : PhysicalEntity
    {
        [RequiredComponent]
        public DialogueComponent Dialogue { get; private set; }
    }
}
