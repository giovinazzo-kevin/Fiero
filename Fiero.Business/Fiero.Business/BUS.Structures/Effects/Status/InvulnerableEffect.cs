using Unconcern.Common;

namespace Fiero.Business
{
    public sealed class InvulnerableEffect : TypedEffect<Actor>
    {
        public InvulnerableEffect(Entity source) : base(source)
        {
        }

        public override EffectName Name => EffectName.Invulnerable;
        public override string DisplayName => "$Effect.Invulnerable.Name$";
        public override string DisplayDescription => "$Effect.Invulnerable.Desc$";
        protected override void TypedOnStarted(GameSystems systems, Actor target)
        {
            target.ActorProperties.Health.Lock = true;
        }
        protected override void TypedOnEnded(GameSystems systems, Actor target)
        {
            target.ActorProperties.Health.Lock = false;
        }
        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield break;
        }
    }
}
