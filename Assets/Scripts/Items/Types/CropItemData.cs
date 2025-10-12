using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items 
{
    [CreateAssetMenu(fileName = "Crop", menuName = "Game/Item/Crop Item Data")]
    public class CropItemData : ItemData
    {
        [Header("Crop Specific")]
        [Tooltip("Loại nông sản")]
        public CropType cropType;

        [Tooltip("Mùa thu hoạch")]
        public Season harvestedSeason;

        [Header("Nutrition (nếu ăn được)")]
        [Tooltip("Giá trị dinh dưỡng")]
        [Range(0, 100)]
        public int nutritionValue = 10;

        [Tooltip("Năng lượng hồi phục")]
        [Range(0, 100)]
        public int energyValue = 5;

        [Header("Processing")]
        [Tooltip("Có thể chế biến thành món khác không?")]
        public bool canBeProcessed = false;

        [Tooltip("ID của item sau khi chế biến")]
        public string processedItemID;

        public override ItemType GetItemType()
        {
            return ItemType.Crop;
        }

        protected override void ValidateItemData()
        {
            base.ValidateItemData();

            // Crop có thể stack
            canBeStacked = true;

            // Crop thường có thể ăn (consumable)
            if (nutritionValue > 0 ||  energyValue > 0)
            {
                canBeConsumable = true;
            }

            // Crop không thể trang bị
            canBeEquippable = false;
        }

        // Lấy loại crop
        public CropType GetCropType()
        {
            return cropType;
        }

        // Lấy mùa thu hoạch
        public Season GetHarvestedSeason()
        {
            return harvestedSeason;
        }

        // Lấy giá trị dinh dưỡng
        public int GetNutritionValue()
        {
            return nutritionValue;
        }

        // Lấy năng lượng hồi phục
        public int GetEnergyValue()
        {
            return energyValue;
        }

        // Có thể chế biến không?
        public bool CanBeProcessed()
        {
            return canBeProcessed && !string.IsNullOrEmpty(processedItemID);
        }
    }
}


