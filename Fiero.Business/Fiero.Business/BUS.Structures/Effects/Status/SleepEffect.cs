using Fiero.Core;
using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    // Unable to act, but you slowly restore your HP. Woken up after several turns; increasing chance of waking up when taking lots of damage.
    public class SleepEffect : StatusEffect
    {
        public override string DisplayName => "$Effect.Sleep.Name$";
        public override string DisplayDescription => "$Effect.Sleep.Desc$";
        public override EffectName Name => EffectName.Sleep;

        protected override void Apply(GameSystems systems, Actor target) 
        {
            target.TryRoot();
            Ended += e => target.TryFree();
        }

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            if(!owner.TryCast<Actor>(out var actor)) {
                yield break;
            }

            yield return systems.Action.ActorIntentSelected.SubscribeResponse(e => {
                if (e.Actor == owner) {
                    systems.Action.ActorHealed.Raise(new(e.Actor, e.Actor, e.Actor, Rng.Random.Between(1, 3)));
                    return new(new WaitAction());
                }
                return new();
            });
            yield return systems.Action.ActorAttacked.SubscribeHandler(e => {
                if (e.Victim == owner && Rng.Random.NChancesIn(e.Damage, 100)) {
                    End();
                }
            });
        }
    }
}
