namespace Fiero.Business
{
    public class GrantedOnQuaff : QuaffEffect
    {
        public override string DisplayName => $"$Effect.{Source.Name}$";
        public override string DisplayDescription => "$Effect.GrantedOnQuaff$";
        public override EffectName Name => Source.Name;

        public GrantedOnQuaff(EffectDef source) : base(source)
        {
        }

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            Source.Resolve().Start(systems, target);
        }
    }
}
