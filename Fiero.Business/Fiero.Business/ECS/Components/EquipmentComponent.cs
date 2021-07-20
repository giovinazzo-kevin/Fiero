using Fiero.Core;
using System;
using System.Collections.Generic;

namespace Fiero.Business
{
    public class EquipmentComponent : EcsComponent
    {
        public Weapon LeftHandWeapon { get; private set; }
        public Weapon RightHandWeapon { get; private set; }

        public Armor HeadSlot { get; private set; }
        public Armor TorsoSlot { get; private set; }
        public Armor ArmsSlot { get; private set; }
        public Armor LegsSlot { get; private set; }

        public bool IsEquipped(Item i) =>
            LeftHandWeapon?.Id == i.Id
            || RightHandWeapon?.Id == i.Id
            || HeadSlot?.Id == i.Id
            || TorsoSlot?.Id == i.Id
            || ArmsSlot?.Id == i.Id
            || LegsSlot?.Id == i.Id
            ;

        public IEnumerable<Weapon> GetEquipedWeapons(Func<WeaponComponent, bool> filter = null)
        {
            filter ??= _ => true;
            if (LeftHandWeapon != null && filter(LeftHandWeapon.WeaponProperties))
                yield return LeftHandWeapon;
            if (RightHandWeapon != LeftHandWeapon && RightHandWeapon != null && filter(RightHandWeapon.WeaponProperties))
                yield return RightHandWeapon;
        }

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
            if (i.Id == LeftHandWeapon?.Id)  return UnequipLeftHandWeapon();
            if (i.Id == RightHandWeapon?.Id) return UnequipRightHandWeapon();
            if (i.Id == HeadSlot?.Id)  return UnequipHeadSlot();
            if (i.Id == TorsoSlot?.Id) return UnequipTorsoSlot();
            if (i.Id == ArmsSlot?.Id)  return UnequipArmsSlot();
            if (i.Id == LegsSlot?.Id)  return UnequipLegsSlot();
            return false;
        }

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
