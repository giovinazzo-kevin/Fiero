using System.Collections.Generic;
using Unconcern.Common;

namespace Fiero.Business
{
    /// <summary>
    /// Equipment effects can be applied to:
    /// - Weapons, in which case the effect starts when the weapon is wielded and ends when the weapon is sheated
    /// - Armor, in which case the effect starts when the armor is worn and ends when the armor is removed
    /// </summary>
    public abstract class EquipmentEffect : ModifierEffect
    {
        protected EquipmentEffect(EffectDef source) : base(source)
        {
        }

        protected abstract void OnApplied(GameSystems systems, Actor target);
        protected abstract void OnRemoved(GameSystems systems, Actor target);

        protected override IEnumerable<Subscription> RouteEvents(GameSystems systems, Entity owner)
        {
            if (owner.TryCast<Weapon>(out _) || owner.TryCast<Armor>(out _))
            {
                yield return systems.Action.ItemEquipped.SubscribeHandler(e =>
                {
                    if (e.Item == owner)
                    {
                        OnApplied(systems, e.Actor);
                    }
                });
                yield return systems.Action.ItemUnequipped.SubscribeHandler(e =>
                {
                    if (e.Item == owner)
                    {
                        OnRemoved(systems, e.Actor);
                    }
                });
                // TODO: End the effect when the item is destroyed, if I end up adding a way to destroy items
            }
        }
    }
}
