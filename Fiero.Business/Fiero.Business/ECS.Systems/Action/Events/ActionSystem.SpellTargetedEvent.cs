namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct SpellTargetedEvent
        {
            public readonly Actor Actor;
            public readonly Spell Spell;
            public SpellTargetedEvent(Actor actor, Spell spell)
                => (Actor, Spell) = (actor, spell);
        }
    }
}
