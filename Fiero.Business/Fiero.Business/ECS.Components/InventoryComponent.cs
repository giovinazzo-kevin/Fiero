using Fiero.Core;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class InventoryComponent : EcsComponent
    {
        protected readonly List<Func<Item, bool>> IdentificationRules;
        protected readonly List<Item> Items;

        public int Count => Items.Count;
        public int Capacity { get; set; } = 0;
        public bool Full => Capacity > 0 && Count == Capacity;
        public bool Empty => Count == 0;

        public void AddIdentificationRule(Func<Item, bool> rule)
        {
            IdentificationRules.Add(rule);
        }

        public bool TryIdentify(Item i)
        {
            if (i.ItemProperties.Identified)
            {
                return false;
            }
            if (IdentificationRules.Any(r => r(i)))
            {
                i.ItemProperties.Identified = true;
                return true;
            }
            return false;
        }

        public bool TryPut(Item i, out bool fullyMerged)
        {
            fullyMerged = false;
            if (Capacity <= 0 || Count < Capacity)
            {
                // Merge charges on consumables of the same kind (not wands)
                var merged = false;
                merged |= TryMergeCharges<Projectile>((x, y) => y.ProjectileProperties.Name == x.ProjectileProperties.Name, out fullyMerged);
                if (!merged)
                {
                    merged |= TryMergeCharges<Potion>((x, y) => y.PotionProperties.QuaffEffect.Name == x.PotionProperties.QuaffEffect.Name
                                                             && y.PotionProperties.ThrowEffect.Name == x.PotionProperties.ThrowEffect.Name, out var fully);
                    fullyMerged |= fully;
                }
                if (!merged)
                {
                    merged |= TryMergeCharges<Scroll>((x, y) => y.ScrollProperties.Effect.Name == x.ScrollProperties.Effect.Name
                                                             && y.ScrollProperties.Modifier == x.ScrollProperties.Modifier, out var fully);
                    fullyMerged |= fully;
                }
                if (!merged)
                {
                    merged |= TryMergeResources<Resource>((x, y) => y.ResourceProperties.Name == x.ResourceProperties.Name, out var fully);
                    fullyMerged |= fully;
                }
                if (!merged || !fullyMerged)
                {
                    Items.Add(i);
                }
                return true;
            }
            return false;

            bool TryMergeCharges<T>(Func<T, T, bool> equal, out bool fullyMerged)
                where T : Consumable
            {
                fullyMerged = false;
                if (i.TryCast<T>(out var x) && x.ItemProperties.Identified)
                {
                    while (x.ConsumableProperties.RemainingUses > 0)
                    {
                        var y = Items.TrySelect(x => (x.TryCast<T>(out var e), e)).FirstOrDefault(y =>
                               y.ItemProperties.Identified
                            && y.ConsumableProperties.RemainingUses < y.ConsumableProperties.MaximumUses
                            && equal(x, y)
                        );
                        if (y is { })
                        {
                            x.ConsumableProperties.RemainingUses--;
                            y.ConsumableProperties.RemainingUses++;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    fullyMerged = true;
                    return true;
                }
                return false;
            }

            bool TryMergeResources<T>(Func<T, T, bool> equal, out bool fullyMerged)
                where T : Resource
            {
                fullyMerged = false;
                if (i.TryCast<T>(out var x) && x.ItemProperties.Identified)
                {
                    while (x.ResourceProperties.Amount > 0)
                    {
                        var y = Items.TrySelect(x => (x.TryCast<T>(out var e), e)).FirstOrDefault(y =>
                               y.ItemProperties.Identified
                            && y.ResourceProperties.Amount < y.ResourceProperties.MaximumAmount
                            && equal(x, y)
                        );
                        if (y is { })
                        {
                            x.ResourceProperties.Amount--;
                            y.ResourceProperties.Amount++;
                        }
                        else
                        {
                            return true;
                        }
                    }
                    fullyMerged = true;
                    return true;
                }
                return false;
            }
        }

        public bool TryTake(Item i)
        {
            return Items.Remove(i);
        }

        public IEnumerable<Item> GetItems() => Items;
        public IEnumerable<Weapon> GetWeapons() => Items.OfType<Weapon>();
        public IEnumerable<Armor> GetArmors() => Items.OfType<Armor>();
        public IEnumerable<Consumable> GetConsumables() => Items.OfType<Consumable>();

        public InventoryComponent()
        {
            Items = new();
            IdentificationRules = new();
        }
    }
}
