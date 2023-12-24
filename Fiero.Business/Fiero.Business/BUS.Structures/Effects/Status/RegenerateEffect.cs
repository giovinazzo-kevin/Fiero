using Unconcern.Common;

namespace Fiero.Business
{

    // Heal a fixed % of your max HP 
    public class RegenerateEffect : TypedEffect<Actor>
    {
        public override string DisplayName => "$Effect.Regenerate.Name$";
        public override string DisplayDescription => "$Effect.Regenerate.Desc$";
        public override EffectName Name => EffectName.Regenerate;
        public readonly float Percentage;

        public RegenerateEffect(Entity source, float percentage)
            : base(source)
        {
            Percentage = percentage;
        }

        protected override void TypedOnStarted(MetaSystem systems, Actor target)
        {
            var hp = (int)(Percentage * target.ActorProperties.Health.Max);
            systems.Get<ActionSystem>().ActorHealed.HandleOrThrow(new(target, target, target, hp));
        }

        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            yield break;
        }
    }
}
