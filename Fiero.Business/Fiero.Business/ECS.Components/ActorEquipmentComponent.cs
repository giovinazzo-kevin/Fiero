using Fiero.Core;
using System.Collections.Generic;
using System.Linq;

namespace Fiero.Business
{
    public class ActorEquipmentComponent : EcsComponent
    {
        protected readonly Dictionary<EquipmentSlotName, Equipment> Dict = new();

        public IEnumerable<KeyValuePair<EquipmentSlotName, Equipment>> EquippedItems => Dict;
        public IEnumerable<Weapon> Weapons => Dict.Values.OfType<Weapon>();
        public Armor Armor => Dict.Values.OfType<Armor>().SingleOrDefault();

        public bool TryGetHeld(out Equipment leftHand, out Equipment rightHand)
        {
            var ret = Dict.TryGetValue(EquipmentSlotName.LeftHand, out leftHand);
            ret |= Dict.TryGetValue(EquipmentSlotName.RightHand, out rightHand);
            return ret;
        }

        public EquipmentSlotName? MapFreeSlot(EquipmentTypeName va) => va switch
        {
            EquipmentTypeName.Weapon2H when !Dict.ContainsKey(EquipmentSlotName.LeftHand) && !Dict.ContainsKey(EquipmentSlotName.RightHand)
                => EquipmentSlotName.LeftHand,
            EquipmentTypeName.Weapon1H when !Dict.ContainsKey(EquipmentSlotName.LeftHand)
                => EquipmentSlotName.LeftHand,
            EquipmentTypeName.Weapon1H when !Dict.ContainsKey(EquipmentSlotName.RightHand)
                => EquipmentSlotName.RightHand,
            EquipmentTypeName.Shield when !Dict.ContainsKey(EquipmentSlotName.RightHand)
                => EquipmentSlotName.RightHand,
            EquipmentTypeName.Helmet when !Dict.ContainsKey(EquipmentSlotName.Head)
                => EquipmentSlotName.Head,
            EquipmentTypeName.Armor when !Dict.ContainsKey(EquipmentSlotName.Torso)
                => EquipmentSlotName.Torso,
            EquipmentTypeName.Gauntlets when !Dict.ContainsKey(EquipmentSlotName.Arms)
                => EquipmentSlotName.Arms,
            EquipmentTypeName.Greaves when !Dict.ContainsKey(EquipmentSlotName.Legs)
                => EquipmentSlotName.Legs,
            EquipmentTypeName.Amulet when !Dict.ContainsKey(EquipmentSlotName.Neck)
                => EquipmentSlotName.Neck,
            EquipmentTypeName.Cape when !Dict.ContainsKey(EquipmentSlotName.Back)
                => EquipmentSlotName.Back,
            EquipmentTypeName.Ring when !Dict.ContainsKey(EquipmentSlotName.LeftRing)
                => EquipmentSlotName.LeftRing,
            EquipmentTypeName.Ring when !Dict.ContainsKey(EquipmentSlotName.RightRing)
                => EquipmentSlotName.RightRing,
            _ => null
        };

        public bool IsEquipped(Equipment i) => Dict.Values.Any(v => v.Id == i.Id);
        public bool TryEquip(Equipment i)
        {
            var slot = MapFreeSlot(i.EquipmentProperties.Type);
            if (slot is null)
                return false;
            return Dict.TryAdd(slot.Value, i);
        }
        public bool TryUnequip(Equipment i)
        {
            foreach (var (k, v) in Dict)
            {
                if (v.Id == i.Id)
                    return Dict.Remove(k);
            }
            return false;
        }
    }
}
