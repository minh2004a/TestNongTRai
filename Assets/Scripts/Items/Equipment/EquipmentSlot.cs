using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    [System.Serializable]
    public class EquipmentSlot
    {
        [SerializeField] private EquipmentSlotType slotType;
        [SerializeField] private EquipmentItem equippedItem;
        [SerializeField] private bool isLocked;

        public EquipmentSlotType SlotType => slotType;
        public EquipmentItem EquippedItem => equippedItem;
        public bool IsLocked => isLocked;

        public bool IsEmpty => equippedItem == null;
        public bool IsOccupied => equippedItem != null;
        public bool CanEquip => !isLocked && IsEmpty;

        public event Action<EquipmentItem> OnItemEquipped;
        public event Action<EquipmentItem> OnItemUnequipped;
        public event Action<bool> OnSlotLockChanged;

        public EquipmentSlot(EquipmentSlotType type)
        {
            slotType = type;
            equippedItem = null;
            isLocked = false;
        }

        // Trang bị item vào slot
        /// </summary>
        public bool Equip(EquipmentItem item)
        {
            if (item == null)
            {
                Debug.LogWarning($"[EquipmentSlot] Cannot equip null item!");
                return false;
            }

            if (isLocked)
            {
                Debug.LogWarning($"[EquipmentSlot] Slot {slotType} is locked!");
                return false;
            }

            if (item.SlotType != slotType)
            {
                Debug.LogWarning($"[EquipmentSlot] Item {item.GetDisplayName()} doesn't fit in {slotType} slot!");
                return false;
            }

            if (IsOccupied)
            {
                Debug.LogWarning($"[EquipmentSlot] Slot {slotType} is already occupied!");
                return false;
            }

            // Equip item
            equippedItem = item;
            equippedItem.Equip();

            OnItemEquipped?.Invoke(equippedItem);
            return true;
        }

        // Tháo item khỏi slot
        public EquipmentItem Unequip()
        {
            if (IsEmpty)
            {
                Debug.LogWarning($"[EquipmentSlot] Slot {slotType} is already empty!");
                return null;
            }

            EquipmentItem unequippedItem = equippedItem;
            unequippedItem.Unequip();
            equippedItem = null;

            OnItemUnequipped?.Invoke(unequippedItem);
            return unequippedItem;
        }

        // Thay thế item trong slot (unequip cũ, equip mới)
        public EquipmentItem Replace(EquipmentItem newItem)
        {
            EquipmentItem oldItem = null;

            if (IsOccupied)
            {
                oldItem = Unequip();
            }

            if (newItem != null)
            {
                Equip(newItem);
            }

            return oldItem;
        }

        // Khóa slot (không thể equip/unequip)
        public void Lock()
        {
            isLocked = true;
            OnSlotLockChanged?.Invoke(true);
        }

        // Mở khóa slot
        public void Unlock()
        {
            isLocked = false;
            OnSlotLockChanged?.Invoke(false);
        }

        // Kiểm tra item có thể equip vào slot này không
        public bool CanEquipItem(EquipmentItem item)
        {
            if (item == null) return false;
            if (isLocked) return false;
            if (item.SlotType != slotType) return false;
            return true;
        }

        // Clear slot (force remove)
        public void Clear()
        {
            if (equippedItem != null)
            {
                equippedItem.Unequip();
                equippedItem = null;
            }
        }

        public override string ToString()
        {
            string status = isLocked ? "[LOCKED]" : "";
            string itemName = IsOccupied ? equippedItem.GetDisplayName() : "Empty";
            return $"[EquipmentSlot] {status} {slotType}: {itemName}";
        }
    }
}

