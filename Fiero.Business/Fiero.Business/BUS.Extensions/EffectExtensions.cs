namespace Fiero.Business
{

    public static class EffectExtensions
    {
        public static EffectFlags GetFlags(this EffectName e)
        {
            return e switch
            {
                // buff, debuff, panic, offensive, defensive
                EffectName.Confusion => new(false, true, false, true, false),
                EffectName.Sleep => new(false, true, false, true, false),
                EffectName.Poison => new(false, true, false, true, false),
                EffectName.Entrapment => new(false, true, false, true, true),
                EffectName.Silence => new(false, true, false, true, true),
                EffectName.UncontrolledTeleport => new(false, false, true, false, true),
                EffectName.MagicMapping => new(true, false, false, false, true),
                EffectName.Heal => new(true, false, false, false, true),
                EffectName.Explosion => new(false, false, true, true, false),
                EffectName.RaiseUndead => new(false, false, true, true, true),
                _ => new(false, false, false, false, false)
            };
        }

        public static EffectFlags GetEffectFlags(this Entity e)
        {
            if (e.Effects is null)
                return default;
            return e.Effects.Intrinsic
                .Select(e => e.Name.GetFlags())
                .Aggregate((a, b) => a.Or(b));
        }
    }
}
