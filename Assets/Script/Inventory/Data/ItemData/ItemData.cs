using UnityEngine;

namespace TinyFarm.Items
{
    /// Base ScriptableObject cho tất cả các loại item trong game
    public abstract class ItemData : ScriptableObject
    {
        [Header("Basic Information")]
        [Tooltip("Mã định danh duy nhất của item")]
        public ItemCode itemCode = ItemCode.NoItem;

        [Tooltip("Loại item")]
        public ItemType itemType = ItemType.NoType;

        [Tooltip("Tên hiển thị của item")]
        public string itemName = "no-name";

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

        [Header("Description")]
        [Tooltip("Mô tả ngắn gọn về item")]
        [TextArea(3, 5)]
        public string description;

        public int GetItemID()
        {
            return (int)itemCode;
        }

        /// Kiểm tra xem item có thể xếp chồng với item khác không
        public virtual bool CanStackWith(ItemData other)
        {
            return other != null & this.itemCode == other.itemCode;
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
    }

}
