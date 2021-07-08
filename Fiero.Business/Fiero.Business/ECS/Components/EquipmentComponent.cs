using Fiero.Core;

namespace Fiero.Business
{
    public class EquipmentComponent : Component
    {
        public Weapon LeftHandWeapon { get; private set; }
        public Weapon RightHandWeapon { get; private set; }

        public Armor HeadSlot { get; private set; }
        public Armor TorsoSlot { get; private set; }
        public Armor ArmsSlot { get; private set; }
        public Armor LegsSlot { get; private set; }

        public bool TryEquip(Weapon w)
        {
            if (w.WeaponProperties.Handedness == WeaponHandednessName.OneHanded) {
                if (LeftHandWeapon == null) {
                    LeftHandWeapon = w;
                    return true;
                }
                if (RightHandWeapon == null) {
                    RightHandWeapon = w;
                    return true;
                }
                return false;
            }
            else {
                if (LeftHandWeapon == null && RightHandWeapon == null) {
                    LeftHandWeapon = w;
                    RightHandWeapon = w;
                    return true;
                }
                return false;
            }
        }

        public bool TryEquip(Armor a)
        {
            switch(a.ArmorProperties.Slot) {
                case ArmorSlotName.Head:
                    if (HeadSlot != null) return false;
                    HeadSlot = a; return true;
                case ArmorSlotName.Torso:
                    if (TorsoSlot != null) return false;
                    TorsoSlot = a; return true;
                case ArmorSlotName.Arms:
                    if (ArmsSlot  != null) return false;
                    ArmsSlot = a; return true;
                case ArmorSlotName.Legs:
                    if (LegsSlot != null) return false;
                    LegsSlot = a; return true;
                default: return false;
            }
        }

        public bool UnequipLeftHandWeapon()
        {
            if (LeftHandWeapon != null) {
                if (RightHandWeapon == LeftHandWeapon) {
                    RightHandWeapon = null;
                }
                LeftHandWeapon = null;
                return true;
            }
            return false;
        }
        public bool UnequipRightHandWeapon()
        {
            if (RightHandWeapon != null) {
                if (LeftHandWeapon == RightHandWeapon) {
                    LeftHandWeapon = null;
                }
                RightHandWeapon = null;
                return true;
            }
            return false;
        }

        public bool UnequipHeadSlot()
        {
            if (HeadSlot != null) {
                HeadSlot = null;
                return true;
            }
            return false;
        }
        public bool UnequipTorsoSlot()
        {
            if (TorsoSlot != null) {
                TorsoSlot = null;
                return true;
            }
            return false;
        }
        public bool UnequipArmsSlot()
        {
            if (ArmsSlot != null) {
                ArmsSlot = null;
                return true;
            }
            return false;
        }
        public bool UnequipLegsSlot()
        {
            if (LegsSlot != null) {
                LegsSlot = null;
                return true;
            }
            return false;
        }
    }
}
