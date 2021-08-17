using System.Linq;

namespace Fiero.Business
{
    public static class EffectExtensions
    {
        public static EffectFlags GetFlags(this EffectName e)
        {
            return e switch {
                EffectName.Confusion => new(false, true, false),
                EffectName.Sleep => new(false, true, false),
                EffectName.Poison => new(false, true, false),
                EffectName.Entrapment => new(false, true, false),
                EffectName.Silence => new(false, true, false),
                EffectName.UncontrolledTeleport => new(false, false, true),
                _ => new(false, false, false)
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

        public static bool IsHelpful(this EffectFlags f) => !f.IsDebuff && f.IsBuff;
        public static bool IsHarmful(this EffectFlags f) => f.IsDebuff && !f.IsBuff;
    }
}
