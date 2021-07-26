using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    /// <summary>
    /// Use effects can be applied to:
    /// - Consumables, in which case the effect starts and ends when the consumable is used
    /// </summary>
    public abstract class UseEffect : Effect
    {
        protected abstract void OnApplied(GameSystems systems, Entity owner, Actor target);

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            if (owner.TryCast<Consumable>(out var consumable)) {
                yield return systems.Action.ItemConsumed.SubscribeHandler(e => {
                    if (e.Item == owner) {
                        OnApplied(systems, owner, e.Actor);
                        if (consumable.ConsumableProperties.RemainingUses <= 0) {
                            End();
                        }
                    }
                });
            }
        }
    }
}
