using UnityEngine;

namespace TinyFarm.Items
{
    [CreateAssetMenu(fileName = "New Resource", menuName = "Game/Item/Resource Item Data")]
    public class ResourcesItemData : ItemData
    {
        [Header("Resource Specific")]
        [Tooltip("Loại tài nguyên")]
        public ResourcesType resourceType;

        [Header("Drop Info")]
        [Tooltip("Số lượng drop (min)")]
        [Range(1, 10)]
        public int minDropAmount = 1;

        [Tooltip("Số lượng drop (max)")]
        [Range(1, 10)]
        public int maxDropAmount = 3;


        public override ItemType GetItemType()
        {
            return ItemType.Resources;
        }

        protected override void ValidateItemData()
        {
            base.ValidateItemData();

            // Resource có thể stack
            canBeStacked = true;

            // Resource không thể ăn hoặc trang bị
            canBeConsumable = false;
            canBeEquippable = false;

            // Đảm bảo maxDropAmount >= minDropAmount
            if (maxDropAmount < minDropAmount)
            {
                maxDropAmount = minDropAmount;
            }
        }

        /// Lấy loại resource
        public ResourcesType GetResourceType()
        {
            return resourceType;
        }

        /// Tính số lượng drop (random)
        public int CalculateDropAmount()
        {
            return Random.Range(minDropAmount, maxDropAmount + 1);
        }
    }
}

