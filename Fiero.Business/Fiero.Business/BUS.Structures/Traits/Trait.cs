using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public readonly record struct Trait(TraitName Name, EffectDef Effect, params TraitName[] Category)
    {
        public override int GetHashCode() => Category.GetHashCode();
    }
    public static class Traits
    {
        private static readonly TraitName[] SizeTraits = new[] { TraitName.Tiny, TraitName.Small, TraitName.Large, TraitName.Huge };
        public static readonly Trait Tiny = new(TraitName.Tiny, new EffectDef(EffectName.IncreaseMaxHP, "-10"), SizeTraits);
        public static readonly Trait Small = new(TraitName.Small, new EffectDef(EffectName.IncreaseMaxHP, "-5"), SizeTraits);
        public static readonly Trait Large = new(TraitName.Large, new EffectDef(EffectName.IncreaseMaxHP, "5"), SizeTraits);
        public static readonly Trait Huge = new(TraitName.Huge, new EffectDef(EffectName.IncreaseMaxHP, "10"), SizeTraits);

        private static readonly List<Trait> All = new()
        {
            Tiny,
            Small,
            Large,
            Huge,
        };

        public static Trait Get(TraitName name) => All.First(x => x.Name == name);
    }
}