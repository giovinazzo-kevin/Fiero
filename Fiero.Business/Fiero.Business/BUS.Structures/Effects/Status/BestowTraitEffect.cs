using System;
using System.Collections.Generic;
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

        protected override void Apply(GameSystems systems, Entity target)
        {
            if (target.Traits is null)
                return;
            target.Traits.AddExtrinsicTrait(Trait);
            var effect = Trait.Effect.Resolve(Source);
            effect.Start(systems, target);
            _onEnded.Add(() =>
            {
                effect.End(systems, target);
                target.Traits.RemoveExtrinsicTrait(Trait);
            });
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield break;
        }
    }
}
