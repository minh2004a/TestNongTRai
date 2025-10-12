using UnityEngine;

namespace TinyFarm.Items
{
    [CreateAssetMenu(fileName = "New Seed", menuName = "Game/Item/Seed Item Data")]
    public class SeedItemData : ItemData
    {
        [Header("Seed Specific")]
        [Tooltip("ID của crop sẽ mọc ra")]
        public string cropID;

        [Header("Season Requirements")]
        [Tooltip("Mùa có thể trồng")]
        public Season validSeason;

        [Header("Growth Info")]
        [Tooltip("Số ngày để cây lớn")]
        [Range(1, 28)]
        public int growthDays = 7;

        [Header("Planting Requirements")]
        [Tooltip("Cần tưới nước mỗi ngày?")]
        public bool requiresDailyWatering = true;

        [Header("Yield")]
        [Tooltip("Số lượng crop thu hoạch (min)")]
        [Range(1, 10)]
        public int minYield = 1;

        [Tooltip("Số lượng crop thu hoạch (max)")]
        [Range(1, 10)]
        public int maxYield = 3;

        public override ItemType GetItemType()
        {
            return ItemType.Seed;
        }

        protected override void ValidateItemData()
        {
            base.ValidateItemData();

            // Seed có thể stack
            isStackable = true;

            // Seed không thể ăn hoặc trang bị
            isUsable = false;
            canBeEquippable = false;

            // Đảm bảo maxYield >= minYield
            if (maxYield < minYield)
            {
                maxYield = minYield;
            }

            // Auto-generate cropID nếu chưa có
            if (string.IsNullOrEmpty(cropID))
            {
                cropID = itemID.Replace("Seed_", "Crop_");
            }
        }
        // Lấy ID của crop sẽ mọc ra
        public string GetCropID()
        {
            return cropID;
        }

        // Lấy mùa hợp lệ
        public Season GetValidSeason()
        {
            return validSeason;
        }

        // Có thể trồng trong mùa này không?
        public bool CanPlantInSeason(Season currentseason)
        {
            return validSeason == currentseason;
        }

        // Tính số lượng crop thu hoạch (random trong khoảng min-max)
        public int CalcuateYield()
        {
            return Random.Range(minYield, maxYield + 1);
        }
    }
}

