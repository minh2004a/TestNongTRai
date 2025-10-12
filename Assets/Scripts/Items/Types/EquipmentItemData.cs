using UnityEngine;
using System.Collections.Generic;

namespace TinyFarm.Items
{
    [CreateAssetMenu(fileName = "New Equipment", menuName = "Game/Item/Equipment Item Data")]
    public class EquipmentItemData : ItemData
    {
        [Header("Equipment Specific")]
        [Tooltip("Slot trang b?")]
        public EquipmentSlotType slotType;

        [Header("Base Stats")]
        [Tooltip("Giáp/phong thu")]
        [Range(0, 100)]
        public int defense = 0;

        [Tooltip("Sát thương")]
        [Range(0, 100)]
        public int attack = 0;

        [Tooltip("Toc đo di chuyen (%)")]
        [Range(-50, 100)]
        public int speed = 0;

        [Header("Upgrade")]
        [Tooltip("Công cụ có thể nâng cấp không")]
        public bool canBeUpgraded = true;

        [Tooltip("Công cụ nâng cấp tiếp theo")]
        public EquipmentItemData upgradedVersion;

        [Tooltip("Chi phí nâng cấp")]
        public int upgradeCost = 0;

        [Header("Additional Stats")]
        [Tooltip("Stats bo sung (VD: 'health': 50, 'stamina': 20)")]
        public Dictionary<string, int> additionalStats = new Dictionary<string, int>();

        public override ItemType GetItemType()
        {
            return ItemType.Equipment;
        }

        protected override void ValidateItemData()
        {
            base.ValidateItemData();

            // Equipment không thể stack
            canBeStacked = false;
            maxStack = 1;

            // Equipment có thể trang bị
            canBeEquippable = true;

            // Equipment không thể ăn
            canBeConsumable = false;
        }

        /// Lấy tất cả stats dưới dạng Dictionary
        public Dictionary<string, int> GetAllStats()
        {
            Dictionary<string, int> stats = new Dictionary<string, int>();

            // Base stats
            if (defense > 0) stats["defense"] = defense;
            if (attack > 0) stats["attack"] = attack;
            if (speed != 0) stats["speed"] = speed;

            // Additional stats
            foreach (var stat in additionalStats)
            {
                stats[stat.Key] = stat.Value;
            }

            return stats;
        }

        /// Lấy giá trị của stat cụ thể
        public int GetStatValue(string statName)
        {
            switch (statName.ToLower())
            {
                case "defense": return defense;
                case "attack": return attack;
                case "speed": return speed;
                default:
                    if (additionalStats.ContainsKey(statName))
                    {
                        return additionalStats[statName];
                    }
                    return 0;
            }
        }
    }
}

