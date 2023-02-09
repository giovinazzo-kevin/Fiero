namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct CorpseRaisedEvent(Entity Source, Corpse Corpse, bool RaisedAsZombie);
    }
}
