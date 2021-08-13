namespace Fiero.Business
{
    public class GrantedOnRead : ReadEffect
    {
        public override string Name => $"$Effect.{Source.Name}$";
        public override string Description => "$Effect.GrantedOnRead$";
        public override EffectName Type => Source.Name;

        public GrantedOnRead(EffectDef source) : base(source)
        {
        }

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            Source.Resolve().Start(systems, target);
        }
    }
}
