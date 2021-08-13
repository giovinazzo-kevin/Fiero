using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    /// <summary>
    /// Use effects can be applied to:
    /// - Scrolls:
    ///     - The effect is applied to the actor that reads the scroll.
    /// </summary>
    public abstract class ReadEffect : ModifierEffect
    {
        protected ReadEffect(EffectDef source) : base(source)
        {
        }

        protected abstract void OnApplied(GameSystems systems, Entity owner, Actor target);

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            if (owner.TryCast<Scroll>(out var scroll)) {
                yield return systems.Action.ScrollRead.SubscribeHandler(e => {
                    if (e.Scroll == owner) {
                        OnApplied(systems, owner, e.Actor);
                    }
                });
            }
        }
    }
}
