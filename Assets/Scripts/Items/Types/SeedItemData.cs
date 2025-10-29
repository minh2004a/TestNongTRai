using UnityEngine;

namespace TinyFarm.Items
{
    [CreateAssetMenu(fileName = "New Seed", menuName = "Game/Item/Seed Item Data")]
    public class SeedItemData : ItemData
    {
        [Header("Crop Reference")]
        [Tooltip("Crop Data khi gieo hạt này")]
        public CropData cropData;

        [Header("Planting Rules")]
        [Tooltip("Cần tưới nước mỗi ngày?")]
        public bool requiresDailyWatering = true;

        public override ItemType GetItemType()
        {
            return ItemType.Seed;
        }

        protected override void ValidateItemData()
        {
            base.ValidateItemData();

            // Seed có thể stack nhưng không trang bị hay sử dụng trực tiếp
            isStackable = true;
            isUsable = true;
            canBeEquippable = false;

            if (cropData == null)
            {
                Debug.LogWarning($"[SeedItemData] {itemName} không có cropData liên kết!");
            }
        }

        // Helper API
        public string GetCropID() => cropData != null ? cropData.cropID : string.Empty;
        public bool CanPlantInSeason(Season current) => cropData != null && cropData.IsValidSeason(current);
    }
}

