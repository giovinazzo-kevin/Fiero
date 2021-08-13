namespace Fiero.Business
{
    public class GrantedWhenHitByMeleeWeapon : HitDealtEffect
    {
        public GrantedWhenHitByMeleeWeapon(EffectDef source) : base(source)
        {
        }

        public override string Name => $"$Effect.{Source.Name}$";
        public override string Description => "$Effect.GrantedWhenHitByMeleeWeapon$";
        public override EffectName Type => Source.Name;

        protected override void OnApplied(GameSystems systems, Entity owner, Actor source, Actor target, int damage)
        {
            Source.Resolve().Start(systems, target);
        }
    }
}
