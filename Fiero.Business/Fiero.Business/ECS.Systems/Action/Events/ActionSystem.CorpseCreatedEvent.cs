namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct CorpseCreatedEvent(Actor Actor, Corpse Corpse);
    }
}
