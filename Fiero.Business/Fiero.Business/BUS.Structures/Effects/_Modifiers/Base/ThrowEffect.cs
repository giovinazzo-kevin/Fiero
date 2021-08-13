using Fiero.Core;
using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    /// <summary>
    /// Use effects can be applied to:
    /// - Throwables:
    ///     - The effect is applied to the actor that's hit by the thrown item.
    /// </summary>
    public abstract class ThrowEffect : ModifierEffect
    {
        protected ThrowEffect(EffectDef source) : base(source)
        {
        }

        protected abstract void OnApplied(GameSystems systems, Entity owner, Actor target);
        protected abstract void OnApplied(GameSystems systems, Entity owner, Coord location);

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            if (owner.TryCast<Throwable>(out var throwable)) {
                yield return systems.Action.ItemThrown.SubscribeHandler(e => {
                    if (e.Item == owner) {
                        if (e.Victim != null) {
                            OnApplied(systems, owner, e.Victim);
                        }
                        else {
                            OnApplied(systems, owner, e.Position);
                        }
                    }
                });
            }
        }
    }
}
