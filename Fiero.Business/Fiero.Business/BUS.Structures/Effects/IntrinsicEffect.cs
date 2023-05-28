using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    /// <summary>
    /// Intrinsic effects can be applied to:
    /// - Actors:
    ///     - The effect is applied to the actor when the actor spawns, and it ends when the actor dies.
    /// - Items:
    ///     - The effect is applied to the actor that picks up the item, and it ends when the actor drops the item.
    /// - Spells:
    ///     - The effect is applied to the actor that learns the spell, and it ends when the actor forgets the spell.
    /// </summary>
    public abstract class IntrinsicEffect : Effect
    {
        protected abstract void OnApplied(GameSystems systems, Entity owner, Actor target);
        protected abstract void OnRemoved(GameSystems systems, Entity owner, Actor target);

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            if (owner.TryCast<Actor>(out var target))
            {
                yield return systems.Action.ActorSpawned.SubscribeHandler(e =>
                {
                    if (e.Actor == target)
                    {
                        OnApplied(systems, owner, e.Actor);
                    }
                });
                // Don't bind to the ActorDespawned event, because it invalidates the owner
                yield return systems.Action.ActorDied.SubscribeHandler(e =>
                {
                    if (e.Actor == target)
                    {
                        OnRemoved(systems, owner, e.Actor);
                        End(systems, owner);
                    }
                });
            }
            else if (owner.TryCast<Item>(out var item))
            {
                yield return systems.Action.ItemPickedUp.SubscribeHandler(e =>
                {
                    if (e.Item == item)
                    {
                        OnApplied(systems, owner, e.Actor);
                    }
                });
                yield return systems.Action.ItemDropped.SubscribeHandler(e =>
                {
                    if (e.Item == item)
                    {
                        OnRemoved(systems, owner, e.Actor);
                    }
                });
                // TODO: End the effect when the item is destroyed, if I end up adding a way to destroy items
            }
            else if (owner.TryCast<Spell>(out var spell))
            {
                yield return systems.Action.SpellLearned.SubscribeHandler(e =>
                {
                    if (e.Spell == spell)
                    {
                        OnApplied(systems, owner, e.Actor);
                    }
                });
                yield return systems.Action.SpellForgotten.SubscribeHandler(e =>
                {
                    if (e.Spell == spell)
                    {
                        OnRemoved(systems, owner, e.Actor);
                        End(systems, owner); // TODO: If spells become singletons, remove this
                    }
                });
            }
        }
    }
}
