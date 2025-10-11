using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items 
{
    [CreateAssetMenu(fileName = "Crop", menuName = "Data/Item/CropData")]
    public class CropData : ItemData
    {
        [Tooltip("Loại nông sản")]
        public CropCategory category = CropCategory.Vegetable;

        [Tooltip("Có thể ăn được không")]
        public bool isEdible = false;

        //[Header("Thông tin cơ bản")]
        //public string CropName;
        //public Sprite[] growthSprites; // từng giai đoạn phát triển
        //public float[] growthTimes; // thời gian giữa các giai đoạn

        //[Header("Thu hoạch")]
        //public int harvestYield = 1; // So luong thu hoach
        //public ItemData harvestItem; //Item thu duoc khi thu hoach

        public CropData()
        {
            itemType = ItemType.Crop;
            maxStack = 999;
        }

        ///// <summary>
        ///// Tính giá bán theo chất lượng
        ///// </summary>
        //public int GetAdjustedSellPrice()
        //{
        //    return Mathf.RoundToInt(sellPrice * qualityPriceMultiplier);
        //}

        //public override string GetTooltipText()
        //{
        //    string tooltip = base.GetTooltipText();

        //    tooltip += $"\n\n<color=purple>Quality: {quality}</color>";
        //    tooltip += $"\n<color=green>Category: {category}</color>";

        //    if (isEdible)
        //    {
        //        tooltip += "\n\n<color=cyan>Edible</color>";
        //        if (energyRestore > 0)
        //            tooltip += $"\n<color=yellow>Energy: +{energyRestore}</color>";
        //        if (healthRestore > 0)
        //            tooltip += $"\n<color=red>Health: +{healthRestore}</color>";
        //    }

        //    if (canBeProcessed && processedItem != null)
        //    {
        //        tooltip += $"\n\n<color=orange>Can be processed into: {processedItem.itemName}</color>";
        //    }

        //    return tooltip;
        //}
    }

    /// Chất lượng nông sản
    //public enum CropQuality
    //{
    //    Normal,     // Bình thường
    //    Silver,     // Bạc
    //    Gold,       // Vàng
    //    Iridium     // Iridium (chất lượng cao nhất)
    //}

    /// Phân loại nông sản
    public enum CropCategory
    {
        Vegetable,      // Rau củ
        Fruit,          // Hoa quả
        Flower,         // Hoa
        Grain,          // Ngũ cốc
        Other           // Khác
    }
}


