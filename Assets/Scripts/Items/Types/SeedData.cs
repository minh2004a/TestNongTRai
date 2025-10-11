using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    [CreateAssetMenu(fileName = "New Seed", menuName = "Data/Item/SeedData")]
    public class SeedData : ItemData
    {
        [Header("Seed Specific")]
        [Tooltip("Loại cây trồng sẽ mọc từ hạt giống này")]
        public CropData cropToGrow;

        [Tooltip("Số ngày cần để cây trồng hoàn thành")]
        [Range(1, 30)]
        public int daysToGrow = 4;

        [Tooltip("Các giai đoạn tăng trưởng của cây (sprite theo từng ngày)")]
        public Sprite[] growStages;

        [Header("Season Requirements")]
        [Tooltip("Mùa có thể trồng")]
        public Season[] validSeasons;

        [Header("Yield")]
        [Tooltip("Số lượng tối thiểu thu hoạch được")]
        [Range(1, 10)]
        public int maxYield = 1;

        public SeedData()
        {
            itemType = ItemType.Seed;
            maxStack = 999;
        }

        public override string GetTooltipText()
        {
            string tooltip = base.GetTooltipText();
            tooltip += $"\n\n<color=green>Growth Time: {daysToGrow} days</color>";

            if (validSeasons != null && validSeasons.Length > 0)
            {
                tooltip += "\n<color=cyan>Seasons: ";
                for (int i = 0; i < validSeasons.Length; i++)
                {
                    tooltip += validSeasons[i].ToString();
                    if (i < validSeasons.Length - 1) tooltip += ", ";
                }
                tooltip += "</color>";
            }

            return tooltip;
        }
    }

    /// Enum cho các mùa trong game
    public enum Season
    {
        Spring,     // Mùa xuân
        Summer,     // Mùa hè
        Fall,       // Mùa thu
        Winter      // Mùa đông
    }
}

