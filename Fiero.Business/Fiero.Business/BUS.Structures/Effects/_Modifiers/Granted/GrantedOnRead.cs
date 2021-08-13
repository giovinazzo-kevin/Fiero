namespace Fiero.Business
{
    public class GrantedOnRead : ReadEffect
    {
        public override string DisplayName => $"$Effect.{Source.Name}$";
        public override string DisplayDescription => "$Effect.GrantedOnRead$";
        public override EffectName Name => Source.Name;

        public GrantedOnRead(EffectDef source) : base(source)
        {
        }

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            Source.Resolve().Start(systems, target);
        }
    }
}
