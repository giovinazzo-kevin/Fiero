using Fiero.Core;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class InventoryComponent : Component
    {
        protected readonly List<Item> Items;

        public int Count => Items.Count;
        public int Capacity { get; set; } = 0;

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
            Items = new List<Item>();
        }
    }
}
