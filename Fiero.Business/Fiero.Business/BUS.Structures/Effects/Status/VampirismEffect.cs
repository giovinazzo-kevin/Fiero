namespace Fiero.Business
{
    public class VampirismEffect : HitDealtEffect
    {
        public readonly int Magnitude;

        public VampirismEffect(EffectDef source, int magnitude) : base(source)
        {
            Magnitude = magnitude;
        }

        public override EffectName Name => EffectName.Vampirism;
        public override string DisplayName => "$Effect.Vampirism.Name$";
        public override string DisplayDescription => "$Effect.Vampirism.Desc$";

        protected override void OnApplied(MetaSystem systems, Entity owner, Actor source, Actor target, int damage)
        {
            var fixedHealing = Rng.Random.RoundProportional((Magnitude / 2f));
            // ~1% at magnitude 1, ~30% at magnitude 10
            var percentCurve = 0.3 * Math.Pow(Magnitude, 2) + 1;
            var percentHealing = Rng.Random.RoundProportional((damage * percentCurve / 100f));
            // 50% of healing 1HP at level 1, 100% of 1HP at level 2, 50% of 2HP at level 3, 100% of 2HP at level 4...
            var totalHealing = fixedHealing + percentHealing;
            systems.Get<ActionSystem>().ActorHealed.HandleOrThrow(new(source, source, owner, totalHealing));
        }
    }
}
