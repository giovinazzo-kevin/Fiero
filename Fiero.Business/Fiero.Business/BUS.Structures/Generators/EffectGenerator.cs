using Ergo.Lang.Extensions;

namespace Fiero.Business
{

    public static class EffectGenerator
    {
        public static Func<EffectDef, Effect> GenerateModifier<T>()
            where T : Entity
        {
            var candidates = new List<Func<EffectDef, Effect>>();
            if (typeof(Projectile).IsAssignableFrom(typeof(T)))
                candidates.Add(def => new GrantedWhenHitByThrownItem(def));
            if (typeof(Consumable).IsAssignableFrom(typeof(T)))
                candidates.Add(def => new GrantedOnUse(def));
            if (typeof(Equipment).IsAssignableFrom(typeof(T)))
                candidates.Add(def => new GrantedOnEquip(def));
            if (typeof(Potion).IsAssignableFrom(typeof(T)))
                candidates.Add(def => new GrantedOnQuaff(def));
            if (typeof(Wand).IsAssignableFrom(typeof(T)))
                candidates.Add(def => new GrantedWhenHitByZappedWand(def));
            if (typeof(Scroll).IsAssignableFrom(typeof(T)))
                candidates.Add(def => new GrantedWhenTargetedByScroll(def, Rng.Random.Choose(Enum.GetValues<ScrollModifierName>())));
            if (typeof(Weapon).IsAssignableFrom(typeof(T)))
                candidates.Add(def => new GrantedWhenHitByWeapon(def));
            if (typeof(Feature).IsAssignableFrom(typeof(T)))
                candidates.Add(def => new GrantedWhenSteppedOn(def, isTrap: false, autoRemove: Rng.Random.Choose([true, false])));
            return Rng.Random.Choose(candidates);
        }

        public static EffectDef GenerateDef(int magnitude)
        {
            var name = Rng.Random.Choose(Enum.GetValues<EffectName>()
                .Except([EffectName.AutoPickup, EffectName.MagicMapping, EffectName.Script, EffectName.None]).ToArray());
            return new EffectDef(name, arguments: Arguments(), duration: Duration(), chance: Chance(), canStack: true);
            string Arguments()
            {
                return name switch
                {
                    EffectName.Poison => (Rng.Random.Between(1, 5) * magnitude).ToString(),
                    EffectName.Heal => (Rng.Random.Between(1, 5) * 2 * magnitude).ToString(),
                    EffectName.Regenerate => (Rng.Random.NextDouble()).ToString(),
                    EffectName.Vampirism => (Rng.Random.Between(1, 5) * magnitude).ToString(),
                    EffectName.IncreaseMaxHP => (Rng.Random.Between(1, 10) * magnitude).ToString(),
                    EffectName.IncreaseMaxMP => (Rng.Random.Between(1, 10) * magnitude).ToString(),
                    EffectName.Explosion => (magnitude).ToString(),
                    EffectName.BestowTrait => Rng.Random.Choose(Enum.GetValues<TraitName>()).ToString().ToErgoCase(),
                    EffectName.RemoveTrait => Rng.Random.Choose(Enum.GetValues<TraitName>()).ToString().ToErgoCase(),
                    EffectName.RaiseUndead => Rng.Random.Choose(Enum.GetValues<UndeadRaisingName>()).ToString().ToErgoCase(),
                    _ => null
                };
            }
            int? Duration()
            {
                if (Rng.Random.NChancesIn(1, 2))
                    return null;
                return Rng.Random.Between(1, 10);
            }
            float? Chance()
            {
                if (Rng.Random.NChancesIn(1, 2))
                    return null;
                return (float)Rng.Random.NextDouble();
            }
        }
    }
}
