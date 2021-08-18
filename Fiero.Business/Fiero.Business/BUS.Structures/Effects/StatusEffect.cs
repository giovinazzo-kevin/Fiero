namespace Fiero.Business
{
    /// <summary>
    /// Intrinsic effects can be applied to:
    /// - Actors:
    ///     - The effect is applied to the actor when the effect starts, and is removed when the effect ends.
    /// </summary>
    public abstract class StatusEffect : Effect
    {
        public readonly Entity Source;

        public StatusEffect(Entity source)
        {
            Source = source;
        }

        protected abstract void Apply(GameSystems systems, Actor target);

        protected override void OnStarted(GameSystems systems, Entity owner)
        {
            base.OnStarted(systems, owner);
            if(owner.TryCast<Actor>(out var target)) {
                Apply(systems, target);
            }
        }
    }
}
