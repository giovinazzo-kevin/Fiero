namespace Fiero.Business
{
    public class GrantedOnUse : UseEffect
    {
        public override string Name => $"{Source.Name} ($Effect.GrantedOnUse$)";
        public override string Description => Source.Description;
        public override EffectName Type => Source.Type;

        public readonly Effect Source;
        public GrantedOnUse(Effect source)
        {
            Source = source;
        }

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            Source.Start(systems, target);
        }
    }
}
