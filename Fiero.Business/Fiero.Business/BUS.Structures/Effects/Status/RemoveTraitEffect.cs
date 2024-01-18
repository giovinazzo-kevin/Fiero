using Unconcern.Common;

namespace Fiero.Business
{
    public class RemoveTraitEffect : TypedEffect<Entity>
    {
        public override string DisplayName => "$Effect.RemoveTrait.Name$";
        public override string DisplayDescription => "$Effect.RemoveTrait.Desc$";

        public override EffectName Name => EffectName.RemoveTrait;

        public readonly Trait Trait;
        public RemoveTraitEffect(Entity source, Trait trait) : base(source)
        {
            Trait = trait;
        }

        protected override void TypedOnStarted(MetaSystem systems, Entity target)
        {
            if (target.Traits is null)
                return;
            target.Traits.RemoveExtrinsicTrait(Trait, fireKillSwitch: true);
        }

        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            yield break;
        }
    }
}
