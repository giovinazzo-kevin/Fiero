using Fiero.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace Fiero.Business
{
    public class InventoryComponent : EcsComponent
    {
        protected readonly List<Func<Item, bool>> IdentificationRules;
        protected readonly List<Item> Items;

        public int Count => Items.Count;
        public int Capacity { get; set; } = 0;

        public void AddIdentificationRule(Func<Item, bool> rule)
        {
            IdentificationRules.Add(rule);
        }

        public bool TryIdentify(Item i)
        {
            if(i.ItemProperties.Identified) {
                return false;
            }
            if(IdentificationRules.Any(r => r(i))) {
                i.ItemProperties.Identified = true;
                return true;
            }
            return false;
        }

        public bool TryPut(Item i, out bool fullyMerged)
        {
            fullyMerged = false;
            if (Capacity <= 0 || Count < Capacity) {
                // Merge charges on consumables of the same kind
                var merged = false;
                merged |= TryMergeCharges<Throwable>((x, y) => y.ThrowableProperties.Name == x.ThrowableProperties.Name, out var tMerge);
                merged |= TryMergeCharges<Potion>((x, y) => y.PotionProperties.Name == x.PotionProperties.Name, out var pMerge);
                merged |= TryMergeCharges<Scroll>((x, y) => y.ScrollProperties.Name == x.ScrollProperties.Name, out var sMerge);
                fullyMerged = tMerge || pMerge || sMerge;
                if (!merged || !fullyMerged) {
                    Items.Add(i);
                }
                return true;
            }
            return false;

            bool TryMergeCharges<T>(Func<T, T, bool> equal, out bool fullyMerged)
                where T : Consumable
            {
                fullyMerged = false;
                if (i is T x && x.ItemProperties.Identified) {
                    while(x.ConsumableProperties.RemainingUses > 0) {
                        var same = Items.OfType<T>().FirstOrDefault(y =>
                               y.ItemProperties.Identified
                            && y.ConsumableProperties.RemainingUses < y.ConsumableProperties.MaximumUses
                            && equal(x, y)
                        );
                        if (same is T y) {
                            x.ConsumableProperties.RemainingUses--;
                            y.ConsumableProperties.RemainingUses++;
                        }
                        else {
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
