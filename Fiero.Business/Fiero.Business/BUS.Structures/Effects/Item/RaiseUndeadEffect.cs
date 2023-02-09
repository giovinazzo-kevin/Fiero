using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    // Corpses on the target tile are raised as undead.
    public class RaiseUndeadEffect : TypedEffect<Corpse>
    {
        // If true raises a zombie, otherwise a skeleton
        public readonly bool RaiseAsZombie;

        public RaiseUndeadEffect(Entity source, bool raiseAsZombie) : base(source)
        {
            RaiseAsZombie = raiseAsZombie;
        }

        public override string DisplayName => "$Effect.RaiseUndead.Name$";
        public override string DisplayDescription => "$Effect.RaiseUndead.Desc$";
        public override EffectName Name => EffectName.RaiseUndead;

        protected override void Apply(GameSystems systems, Corpse target)
        {
            systems.Action.CorpseRaised.HandleOrThrow(new(Source, target, RaiseAsZombie));
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield break;
        }
    }
}
