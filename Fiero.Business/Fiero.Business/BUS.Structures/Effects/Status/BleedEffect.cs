using Fiero.Core;
using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    // Causes damage while moving, is healed by standing still for a few turns.
    public class BleedEffect : StatusEffect
    {
        public override string DisplayName => "$Effect.Bleed.Name$";
        public override string DisplayDescription => "$Effect.Bleed.Desc$";
        public override EffectName Name => EffectName.Bleed;

        protected override void Apply(GameSystems systems, Actor target)
        {

        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            yield return systems.Action.ActorMoved.SubscribeHandler(e => {
                if (e.Actor == owner) {
                    systems.Action.ActorDamaged.Raise(new(e.Actor, e.Actor, e.Actor, Rng.Random.Between(1, 3)));
                }
            });
            yield return systems.Action.ActorWaited.SubscribeHandler(e => {
                if (e.Actor == owner && Rng.Random.NChancesIn(1, 4)) {
                    End();
                }
            });
        }
    }
}
