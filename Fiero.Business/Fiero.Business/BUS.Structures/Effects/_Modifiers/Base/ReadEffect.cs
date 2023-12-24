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

        protected abstract void OnApplied(MetaSystem systems, Entity owner, Actor target);

        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            if (owner.TryCast<Scroll>(out var scroll))
            {
                yield return systems.Get<ActionSystem>().ScrollRead.SubscribeHandler(e =>
                {
                    if (e.Scroll == owner)
                    {
                        OnApplied(systems, owner, e.Actor);
                    }
                });
            }
        }
    }
}
