using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    // Take fixed damage over time
    public class PoisonEffect : TypedEffect<Actor>
    {
        public override string DisplayName => "$Effect.Poison.Name$";
        public override string DisplayDescription => "$Effect.Poison.Desc$";
        public override EffectName Name => EffectName.Poison;
        public readonly int Amount;

        public PoisonEffect(Entity source, int amount)
            : base(source)
        {
            Amount = amount;
        }

        protected override void Apply(GameSystems systems, Actor target)
        {
            target.TryRoot();
            Ended += e => target.TryFree();
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            if (!owner.TryCast<Actor>(out var actor))
            {
                yield break;
            }

            yield return systems.Action.ActorIntentSelected.SubscribeResponse(e =>
            {
                if (e.Actor == owner)
                {
                    systems.Action.ActorDamaged.Handle(new(e.Actor, e.Actor, new[] { e.Actor }, Amount));
                }
                return new();
            });
        }
    }
}
