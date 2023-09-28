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

        protected override void OnEnded(GameSystems systems, Entity owner)
        {
            base.OnEnded(systems, owner);
            foreach (var onEnded in _onEnded)
                onEnded();
            _onEnded.Clear();
        }

        protected override void TypedOnStarted(GameSystems systems, Entity target)
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

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield break;
        }
    }

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

        protected override void TypedOnStarted(GameSystems systems, Entity target)
        {
            if (target.Traits is null)
                return;
            target.Traits.RemoveExtrinsicTrait(Trait, fireKillSwitch: true);
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield break;
        }
    }
}
