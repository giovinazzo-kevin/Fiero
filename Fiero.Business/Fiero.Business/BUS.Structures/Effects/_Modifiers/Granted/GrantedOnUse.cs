namespace Fiero.Business
{
    public class GrantedOnUse : UseEffect
    {
        public override string Name => $"$Effect.{Source.Name}$";
        public override string Description => "$Effect.GrantedOnUse$";
        public override EffectName Type => Source.Name;

        public GrantedOnUse(EffectDef source) : base(source)
        {
        }

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            Source.Resolve().Start(systems, target);
        }
    }
}
