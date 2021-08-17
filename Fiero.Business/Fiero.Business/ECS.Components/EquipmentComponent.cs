using Fiero.Core;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class EquipmentComponent : EcsComponent
    {
        public Weapon Weapon { get; private set; }
        public Armor Armor { get; private set; }

        public bool IsEquipped(Item i) =>
            Weapon?.Id == i.Id
            || Armor?.Id == i.Id
            ;

        public bool TryEquip(Item i)
        {
            if (i.TryCast<Weapon>(out var w)) return TryEquip(w);
            if (i.TryCast<Armor> (out var a)) return TryEquip(a);
            return false;
        }

        public bool TryUnequip(Item i)
        {
            if (!IsEquipped(i))
                return false;
            if (i.Id == Weapon?.Id)  return UnequipWeapon();
            if (i.Id == Armor?.Id)  return UnequipArmor();
            return false;
        }

        public bool TryEquip(Weapon w)
        {
            if (Weapon != null) return UnequipWeapon() && TryEquip(w);
            Weapon = w; return true;
        }

        public bool TryEquip(Armor a)
        {
            if (Armor != null) return UnequipArmor() && TryEquip(a);
            Armor = a; return true;
        }

        public bool UnequipWeapon()
        {
            if (Weapon != null) {
                Weapon = null;
                return true;
            }
            return false;
        }

        public bool UnequipArmor()
        {
            if (Armor != null) {
                Armor = null;
                return true;
            }
            return false;
        }
    }
}
