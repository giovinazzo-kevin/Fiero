using System.Diagnostics.CodeAnalysis;

namespace Fiero.Business
{
    public readonly struct Trait
    {
        public readonly TraitName Name;
        public readonly EffectDef Effect;
        public readonly TraitName[] Category;
        public Trait(TraitName name, EffectDef effect, params TraitName[] category)
        {
            Name = name;
            Effect = effect;
            Category = category;
        }
        public Action KillSwitch { get; init; } = () => { };
        public override int GetHashCode() => Category.GetHashCode();
        public override bool Equals([NotNullWhen(true)] object obj)
        {
            if (obj is Trait t)
                return t.Category.SequenceEqual(Category);
            return base.Equals(obj);
        }
    }
    public static class Traits
    {
        private static readonly TraitName[] SizeTraits = new[] { TraitName.Tiny, TraitName.Small, TraitName.Large, TraitName.Huge };
        private static readonly TraitName[] BuffTraits = new[] { TraitName.Invulnerable, TraitName.Impassible };

        public static readonly Trait Tiny = new(TraitName.Tiny, new EffectDef(EffectName.IncreaseMaxHP, "-10"), SizeTraits);
        public static readonly Trait Small = new(TraitName.Small, new EffectDef(EffectName.IncreaseMaxHP, "-5"), SizeTraits);
        public static readonly Trait Large = new(TraitName.Large, new EffectDef(EffectName.IncreaseMaxHP, "5"), SizeTraits);
        public static readonly Trait Huge = new(TraitName.Huge, new EffectDef(EffectName.IncreaseMaxHP, "10"), SizeTraits);
        public static readonly Trait Invulnerable = new(TraitName.Invulnerable, new EffectDef(EffectName.Invulnerable), BuffTraits);
        public static readonly Trait Impassible = new(TraitName.Invulnerable, new EffectDef(EffectName.Invulnerable), BuffTraits);

        private static readonly List<Trait> All = new()
        {
            Tiny,
            Small,
            Large,
            Huge,
            Invulnerable,
            Impassible
        };

        public static Trait Get(TraitName name) => All.First(x => x.Name == name);
    }
}