using Unconcern.Common;

namespace Fiero.Business
{
    /// <summary>
    /// Use effects can be applied to:
    /// - Consumables:
    ///     - The effect is applied to the actor that uses (quaffs/reads/throws...) the consumable.
    /// </summary>
    public abstract class UseEffect : ModifierEffect
    {
        protected UseEffect(EffectDef source) : base(source)
        {
        }

        protected abstract void OnApplied(MetaSystem systems, Entity owner, Actor target);

        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            if (owner.TryCast<Consumable>(out var consumable))
            {
                yield return systems.Get<ActionSystem>().ItemConsumed.SubscribeHandler(e =>
                {
                    if (e.Item == owner)
                    {
                        OnApplied(systems, owner, e.Actor);
                    }
                });
            }
        }
    }
}
