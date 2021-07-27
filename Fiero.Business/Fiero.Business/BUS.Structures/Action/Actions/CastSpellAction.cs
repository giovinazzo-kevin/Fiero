namespace Fiero.Business
{
    public readonly struct CastSpellAction : IAction
    {
        public readonly Spell Spell;

        public CastSpellAction(Spell spell)
        {
            Spell = spell;
        }

        ActionName IAction.Name => ActionName.Attack;
        int? IAction.Cost => 100;
    }
}
