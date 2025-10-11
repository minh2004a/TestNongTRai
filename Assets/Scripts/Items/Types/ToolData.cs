using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    [CreateAssetMenu(fileName = "New Tool", menuName = "Data/Item/ToolData")]
    public class ToolData : ItemData
    {
        public ToolCode toolCode = ToolCode.NoItem;

        [Header("Tool Specific")]
        [Tooltip("Loại công cụ")]
        public ToolType toolType = ToolType.NoType;

        [Tooltip("Độ bền tối đa của công cụ")]
        public int maxDurability = 100;

        [Tooltip("Độ bền của công cụ (-1 = không giới hạn)")]
        public int durability = -1;

        [Tooltip("Sức mạnh của công cụ (dùng cho damage, efficiency, etc.)")]
        [Range(1, 10)]
        public int efficiency = 1;

        [Header("Usage Settings")]
        [Tooltip("Thời gian cooldown giữa các lần sử dụng (giây)")]
        public float useCooldown = 0.5f;

        [Header("Upgrade")]
        [Tooltip("Công cụ có thể nâng cấp không")]
        public bool canBeUpgraded = true;

        [Tooltip("Công cụ nâng cấp tiếp theo")]
        public ToolData upgradedVersion;

        [Tooltip("Chi phí nâng cấp")]
        public int upgradeCost = 0;

        //[Tooltip("Animation clip khi sử dụng công cụ")]
        //public string useAnimationName = "UseTool";

        //[Header("Effects")]
        //[Tooltip("Sound effect khi sử dụng")]
        //public AudioClip useSound;

        public ToolData()
        {
            itemType = ItemType.Tool;
            maxStack = 1; // Tools thường không xếp chồng
            canBeDropped = false; // Tools thường không thể vứt
        }

        // Kiểm tra xem công cụ còn sử dụng được không
        public bool IsUsable()
        {
            return durability != 0;
        }

        // Giảm độ bền khi sử dụng
        public void ConsuneDurability(int amount = 1)
        {
            if (durability > 0) 
            {
                durability -= amount;
                if (durability < 0) durability = 0;
            }
        }

        //public override string GetTooltipText()
        //{
        //    string tooltip = base.GetTooltipText();
        //    tooltip += $"\n\n<color=orange>Type: {toolType}</color>";
        //    tooltip += $"\n<color=cyan>Power: {power}</color>";

        //    if (maxDurability > 0)
        //    {
        //        tooltip += $"\n<color=yellow>Durability: {durability}/{maxDurability}</color>";
        //    }

        //    if (energyCost > 0)
        //    {
        //        tooltip += $"\n<color=red>Energy Cost: {energyCost}</color>";
        //    }

        //    return tooltip;
        //}
    }
}

