namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct SpellCastEvent
        {
            public readonly Actor Actor;
            public readonly Spell Spell;
            public SpellCastEvent(Actor actor, Spell spell)
                => (Actor, Spell) = (actor, spell);
        }
    }
}
