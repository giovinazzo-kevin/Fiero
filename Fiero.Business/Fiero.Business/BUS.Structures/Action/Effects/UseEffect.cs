using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    /// <summary>
    /// Intrinsic effects can be applied to:
    /// - Consumables:
    ///     - The effect is applied to the actor that uses the consumable, and it ends when the consumable is used up.
    /// - Spells:
    ///     - The effect is applied to the actor that casts the spell, and it ends when the actor forgets the spell.
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
            else if (owner.TryCast<Spell>(out var spell)) {
                yield return systems.Action.SpellLearned.SubscribeHandler(e => {
                    if (e.Spell == spell) {
                        Subscriptions.Add(systems.Action.SpellCast.SubscribeHandler(e => {
                            if (e.Spell == spell) {
                                OnApplied(systems, owner, e.Actor);
                            }
                        }));
                    }
                });
                yield return systems.Action.SpellForgotten.SubscribeHandler(e => {
                    if (e.Spell == spell) {
                        End(); // TODO: If spells become singletons, remove this
                    }
                });
            }
        }
    }
}
