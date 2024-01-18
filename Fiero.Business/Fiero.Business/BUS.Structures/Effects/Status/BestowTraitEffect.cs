using Unconcern.Common;

namespace Fiero.Business
{
    public class BestowTraitEffect : TypedEffect<Entity>
    {
        public override string DisplayName => "$Effect.BestowTraitEffect.Name$";
        public override string DisplayDescription => "$Effect.BestowTraitEffect.Desc$";

        public override EffectName Name => EffectName.BestowTrait;

        public readonly Trait Trait;
        private readonly List<Action> _onEnded = new();

        public BestowTraitEffect(Entity source, Trait trait) : base(source)
        {
            Trait = trait;
        }

        protected override void OnEnded(MetaSystem systems, Entity owner)
        {
            base.OnEnded(systems, owner);
            foreach (var onEnded in _onEnded)
                onEnded();
            _onEnded.Clear();
        }

        protected override void TypedOnStarted(MetaSystem systems, Entity target)
        {
            if (target.Traits is null)
                return;
            var effect = Trait.Effect.Resolve(Source);
            var killSwitch = () =>
            {
                effect.End(systems, target);
                target.Traits.RemoveExtrinsicTrait(Trait);
            };
            if (target.Traits.AddExtrinsicTrait(Trait, killSwitch, out var removed))
            {
                removed.KillSwitch();
            }
            effect.Start(systems, target);
            _onEnded.Add(killSwitch);
        }

        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            yield break;
        }
    }
}
