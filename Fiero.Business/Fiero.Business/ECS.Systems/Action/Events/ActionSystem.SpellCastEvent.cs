namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct SpellCastEvent(Actor Actor, Spell Spell, PhysicalEntity[] Targets);
    }
}
