namespace Fiero.Business
{
    public readonly struct CastSpellAction : IAction
    {
        public readonly Spell Spell;
        public readonly TargetingShape TargetingShape;

        public CastSpellAction(Spell spell, TargetingShape target)
        {
            Spell = spell;
            TargetingShape = target;
        }

        ActionName IAction.Name => ActionName.Cast;
        int? IAction.Cost => 100;
    }
}
