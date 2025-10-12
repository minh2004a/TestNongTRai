using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    /// Base ScriptableObject cho tất cả các loại item trong game
    public abstract class ItemData : ScriptableObject
    {
        [Header("Classification")]
        public ItemCategory category;
        public List<string> tags = new List<string>();

        [Header("Basic Information")]
        [Tooltip("Loại item")]
        public ItemType itemType = ItemType.NoType;

        [Header("Basic Information")]
        [Tooltip("Do hiem item")]
        public MaterialTier materialTier;

        [Tooltip("Tên hiển thị của item")]
        public string itemName = "no-name";

        [Header("Basic Info")]
        [Tooltip("Unique ID cho item này")]
        public string itemID;

        [Tooltip("Icon của item trong UI")]
        public Sprite icon;

        [Tooltip("Số lượng tối đa có thể xếp chồng trong 1 slot")]
        [Range(1, 999)]
        public int maxStack = 999;

        [Header("Economy")]
        [Tooltip("Giá mua từ shop")]
        public int buyPrice = 0;

        [Tooltip("Giá bán cho shop")]
        public int sellPrice = 0;

        [Header("Other")]
        [Tooltip("Item có thể bị vứt bỏ không")]
        public bool canBeDropped = true;

        [Tooltip("Item có thể được bán không")]
        public bool canBeSold = true;

        [Tooltip("Item có thể được stack không")]
        public bool canBeStacked = true;

        [Tooltip("Item có thể được tieu thu không")]
        public bool canBeConsumable = true;

        [Tooltip("Item có thể được nang cap không")]
        public bool canBeEquippable = true;

        [Header("Durability")]
        [Tooltip("Item có độ bền không?")]
        public bool hasDurability = false;

        [Tooltip("Độ bền tối đa của item")]
        public int maxDurability = 100;

        [Tooltip("Item bị phá hủy khi hết độ bền?")]
        public bool destroyOnBreak = true;

        [Tooltip("Hệ số chi phí sửa chữa (nếu có)")]
        public float repairCostMultiplier = 1f;

        [Header("Description")]
        [Tooltip("Mô tả ngắn gọn về item")]
        [TextArea(3, 5)]
        public string description = "No description";

        public string GetItemID()
        {
            return itemID;
        }

        public virtual ItemType GetItemType()
        {
            return itemType;
        }

        /// Kiểm tra xem item có thể xếp chồng với item khác không
        public virtual bool CanStackWith(ItemData other)
        {
            if (other  == null) return false;
            if (!canBeStacked) return false;
            return this.itemID == other.itemID;
        }

        /// Lấy thông tin hiển thị tooltip
        public virtual string GetTooltipText()
        {
            string tooltip = $"<b>{itemName}</b>\n\n{description}";

            if (sellPrice > 0)
            {
                tooltip += $"\n\n<color=yellow>Sell Price: {sellPrice}g</color>";
            }

            return tooltip;
        }

        protected virtual void OnValidate()
        {
            this.ValidateItemData();
        }
        protected virtual void ValidateItemData()
        {
            if (maxStack < 1)
                maxStack = 1;

            if (buyPrice < 0)
                buyPrice = 0;

            if (sellPrice < 0)
                sellPrice = 0;

            if (string.IsNullOrEmpty(itemName))
                itemName = "Unnamed Item";

            if (icon == null)
                Debug.LogWarning("Item has no icon", this);
        }
    }
}
