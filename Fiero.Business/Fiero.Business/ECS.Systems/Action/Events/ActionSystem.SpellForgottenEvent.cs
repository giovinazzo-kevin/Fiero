namespace Fiero.Business
{
    public partial class ActionSystem
    {
        public readonly record struct SpellForgottenEvent(Actor Actor, Spell Spell);
    }
}
