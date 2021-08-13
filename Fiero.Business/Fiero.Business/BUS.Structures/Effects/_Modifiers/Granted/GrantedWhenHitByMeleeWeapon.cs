namespace Fiero.Business
{
    public class GrantedWhenHitByMeleeWeapon : HitDealtEffect
    {
        public GrantedWhenHitByMeleeWeapon(EffectDef source) : base(source)
        {
        }

        public override string DisplayName => $"$Effect.{Source.Name}$";
        public override string DisplayDescription => "$Effect.GrantedWhenHitByMeleeWeapon$";
        public override EffectName Name => Source.Name;

        protected override void OnApplied(GameSystems systems, Entity owner, Actor source, Actor target, int damage)
        {
            Source.Resolve().Start(systems, target);
        }
    }
}
