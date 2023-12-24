using Unconcern.Common;

namespace Fiero.Business
{
    /// <summary>
    /// Use effects can be applied to:
    /// - Potions:
    ///     - The effect is applied to the actor that drinks the potion.
    /// </summary>
    public abstract class QuaffEffect : ModifierEffect
    {
        protected QuaffEffect(EffectDef source) : base(source)
        {
        }

        protected abstract void OnApplied(MetaSystem systems, Entity owner, Actor target);

        protected override IEnumerable<Subscription> RouteEvents(MetaSystem systems, Entity owner)
        {
            if (owner.TryCast<Potion>(out var wand))
            {
                yield return systems.Get<ActionSystem>().PotionQuaffed.SubscribeHandler(e =>
                {
                    if (e.Potion == owner)
                    {
                        OnApplied(systems, owner, e.Actor);
                    }
                });
            }
        }
    }
}
