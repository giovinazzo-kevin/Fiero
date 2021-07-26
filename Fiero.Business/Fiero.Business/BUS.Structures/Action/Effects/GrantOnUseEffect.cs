namespace Fiero.Business
{
    public class GrantOnUseEffect : UseEffect
    {
        public readonly Effect Source;
        public GrantOnUseEffect(Effect source)
        {
            Source = source;
        }

        protected override void OnApplied(GameSystems systems, Entity owner, Actor target)
        {
            Source.Start(systems, target);
        }
    }
}
