namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct SpellLearnedEvent
        {
            public readonly Actor Actor;
            public readonly Spell Spell;
            public SpellLearnedEvent(Actor actor, Spell spell)
                => (Actor, Spell) = (actor, spell);
        }
    }
}
