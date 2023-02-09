namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct CorpseDestroyedEvent(Corpse Corpse);
    }
}
