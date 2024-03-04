namespace Fiero.Business
{
    public class GrantedWhenHitByWeapon : HitDealtEffect
    {
        public GrantedWhenHitByWeapon(EffectDef source) : base(source)
        {
        }

        public override string DisplayName => $"$Effect.{Source.Name}$";
        public override string DisplayDescription => "$Effect.GrantedWhenHitByMeleeWeapon$";
        public override EffectName Name => Source.Name;

        protected override void OnApplied(MetaSystem systems, Entity owner, Actor source, Actor target, int damage)
        {
            Source.Resolve(source).Start(systems, target, source);
        }
    }
}
