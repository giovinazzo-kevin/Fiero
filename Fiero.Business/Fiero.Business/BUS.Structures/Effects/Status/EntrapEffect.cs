using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    // Unable to move, wears off on its own.
    public class EntrapEffect : StatusEffect
    {
        public override string DisplayName => "$Effect.Root.Name$";
        public override string DisplayDescription => "$Effect.Root.Desc$";
        public override EffectName Name => EffectName.Entrapment;

        public EntrapEffect(Entity source) : base(source) { }

        protected override void Apply(GameSystems systems, Actor target)
        {
            target.TryRoot();
            Ended += e => target.TryFree();
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield break;
        }
    }
}
