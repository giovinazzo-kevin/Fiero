namespace Fiero.Business
{
    public abstract class ModifierEffect : Effect
    {
        public readonly EffectDef Source;

        public ModifierEffect(EffectDef source)
        {
            Source = source;
        }
    }
}
