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

        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            if (!owner.TryCast<Actor>(out var actor))
            {
                yield break;
            }

            yield return systems.Get<ActionSystem>().ActorTurnStarted.SubscribeHandler(e =>
            {
                if (e.Actor == owner)
                {
                    var amount = Amount;
                    if (e.Actor.ActorProperties.Health.V == 1)
                    {
                        amount = 0;
                    }
                    else if (e.Actor.ActorProperties.Health.V <= Amount)
                    {
                        amount = e.Actor.ActorProperties.Health.V - 1;
                    }
                    systems.Get<ActionSystem>().ActorDamaged.Handle(new(e.Actor, e.Actor, new[] { e.Actor }, amount));
                }
            });
        }
    }
}
