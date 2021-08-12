namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly struct SpellCastEvent
        {
            public readonly Actor Actor;
            public readonly Spell Spell;
            public readonly PhysicalEntity[] Targets;
            public SpellCastEvent(Actor actor, Spell spell, PhysicalEntity[] targets)
                => (Actor, Spell, Targets) = (actor, spell, targets);
        }
    }
}
