namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct SpellForgottenEvent
        {
            public readonly Actor Actor;
            public readonly Spell Spell;
            public SpellForgottenEvent(Actor actor, Spell spell)
                => (Actor, Spell) = (actor, spell);
        }
    }
}
