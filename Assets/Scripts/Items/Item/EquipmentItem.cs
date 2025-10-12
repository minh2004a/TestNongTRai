using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    [Serializable]
    public class EquipmentItem : Item
    {
        private EquipmentItemData equipmentData;

        // State
        private bool isEquipped = false;

        // Properties
        public EquipmentSlotType EquipmentSlotType => equipmentData?.slotType ?? EquipmentSlotType.None;
        public EquipmentType EquipmentType => equipmentData?.equipType ?? EquipmentType.None;
        public bool IsEquipped => isEquipped;

        // Stats
        public int AttackBonus => equipmentData?.attack ?? 0;
        public int DefenseBonus => equipmentData?.defense ?? 0;
        public int HealthBonus => equipmentData?.health ?? 0;
        public float SpeedBonus => equipmentData?.speed ?? 0f;

        // Events
        public event Action<EquipmentItem> OnEquipped;
        public event Action<EquipmentItem> OnUnequipped;

        // Constructor
        public EquipmentItem(EquipmentItemData data) : base(data)
        {
            this.equipmentData = data;
        }

        public EquipmentItem(EquipmentItem other) : base(other)
        {
            this.equipmentData = other.equipmentData;
            this.isEquipped = false; // Copy không giữ equipped state
        }

        /// Equip item
        public bool Equip()
        {
            if (isEquipped)
            {
                Debug.Log($"{Name} is already equipped!");
                return false;
            }

            if (!CanUse)
            {
                Debug.Log($"Cannot equip {Name} - item is broken!");
                return false;
            }

            isEquipped = true;
            SetCustomData("IsEquipped", true);
            OnEquipped?.Invoke(this);
            Debug.Log($"Equipped {Name}");
            return true;
        }

        /// Unequip item
        public bool Unequip()
        {
            if (!isEquipped)
            {
                Debug.Log($"{Name} is not equipped!");
                return false;
            }

            isEquipped = false;
            SetCustomData("IsEquipped", false);
            OnUnequipped?.Invoke(this);
            Debug.Log($"Unequipped {Name}");
            return true;
        }

        /// <summary>
        /// Sử dụng equipment (weapon attack, tool use, etc.)
        /// </summary>
        public override bool Use()
        {
            if (!isEquipped)
            {
                Debug.Log($"Must equip {Name} before using!");
                return false;
            }

            if (!base.Use())
                return false;

            // Equipment-specific use logic
            if (EquipmentType == EquipmentType.Weapon)
            {
                Debug.Log($"Attacked with {Name}! Damage: {AttackBonus}");
            }

            return true;
        }

        /// Lấy tổng stats bonus
        public string GetStatsText()
        {
            string stats = "";
            if (AttackBonus > 0) stats += $"+{AttackBonus} ATK ";
            if (DefenseBonus > 0) stats += $"+{DefenseBonus} DEF ";
            if (HealthBonus > 0) stats += $"+{HealthBonus} HP ";
            if (SpeedBonus > 0) stats += $"+{SpeedBonus:F1}% SPD ";
            return stats.Trim();
        }

        public override string ToString()
        {
            string equipped = isEquipped ? " [E]" : "";
            string stats = GetStatsText();
            return $"{Name}{equipped} ({stats})";
        }
    }
}


