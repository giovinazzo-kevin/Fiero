namespace Fiero.Business
{
    public class GrantedOnQuaff : QuaffEffect
    {
        public override string Name => $"$Effect.{Source.Name}$";
        public override string Description => "$Effect.GrantedOnQuaff$";
        public override EffectName Type => Source.Name;

        public GrantedOnQuaff(EffectDef source) : base(source)
        {
        }

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            Source.Resolve().Start(systems, target);
        }
    }
}
