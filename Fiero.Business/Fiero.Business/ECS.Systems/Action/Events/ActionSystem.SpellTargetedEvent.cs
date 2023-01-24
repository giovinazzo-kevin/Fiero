namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct SpellTargetedEvent(Actor Actor, Spell Spell);
    }
}
