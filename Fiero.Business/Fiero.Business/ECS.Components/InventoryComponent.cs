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

        public bool TryPut(Item i)
        {
            if (Capacity <= 0 || Count < Capacity) {
                Items.Add(i);
                return true;
            }
            return false;
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
