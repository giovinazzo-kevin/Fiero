using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    /// <summary>
    /// Intrinsic effects can be applied to:
    /// - Actors, in which case the effect starts when the actor spawns and ends when it dies
    /// - Items, in which case the effect starts when an actor picks up the item, and ends when the item is dropped
    /// </summary>
    public abstract class IntrinsicEffect : Effect
    {
        protected abstract void OnApplied(GameSystems systems, Entity owner, Actor target);
        protected abstract void OnRemoved(GameSystems systems, Entity owner, Actor target);

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            if(owner.TryCast<Actor>(out var target)) {
                yield return systems.Action.ActorSpawned.SubscribeHandler(e => {
                    if (e.Actor == target) {
                        OnApplied(systems, owner, e.Actor);
                    }
                });
                // Don't bind to the ActorDespawned event, because it invalidates the owner
                yield return systems.Action.ActorDied.SubscribeHandler(e => {
                    if (e.Actor == target) {
                        OnRemoved(systems, owner, e.Actor);
                        End();
                    }
                });
            }
            else if (owner.TryCast<Item>(out var item)) {
                yield return systems.Action.ItemPickedUp.SubscribeHandler(e => {
                    if (e.Item == item) {
                        OnApplied(systems, owner, e.Actor);
                    }
                });
                yield return systems.Action.ItemDropped.SubscribeHandler(e => {
                    if (e.Item == item) {
                        OnRemoved(systems, owner, e.Actor);
                    }
                });
                // TODO: End the effect when the item is destroyed, if I end up adding a way to destroy items
            }
        }
    }
}
