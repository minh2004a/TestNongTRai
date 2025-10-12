using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    /// Base ScriptableObject cho tất cả các loại item trong game
    public abstract class ItemData : ScriptableObject
    {
        // BASIC INFORMATION
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

        [Header("Basic Information")]
        [Tooltip("Loại item")]
        public ItemType itemType = ItemType.NoType;

        [Header("Description")]
        [Tooltip("Mô tả ngắn gọn về item")]
        [TextArea(3, 5)]
        public string description = "No description";

        // STACKABLE SETTINGS (cho StackableProperty)
        [Tooltip("Số lượng tối đa có thể xếp chồng trong 1 slot")]
        [Range(1, 999)]
        public int maxStackSize = 999;

        [Tooltip("Item có thể được stack không")]
        public bool isStackable = true;

        // DURABILITY SETTINGS (cho DurabilityProperty)
        [Header("Durability")]
        [Tooltip("Item có độ bền không?")]
        public bool hasDurability = false;

        [Tooltip("Độ bền tối đa của item")]
        public int maxDurability = 100;

        [Tooltip("Item bị phá hủy khi hết độ bền?")]
        public bool destroyOnBreak = true;

        [Tooltip("Hệ số chi phí sửa chữa (nếu có)")]
        public float repairCostMultiplier = 1f;

        [Tooltip("Độ bền mất mỗi lần sử dụng")]
        [Min(0)]
        public float durabilityLossPerUse = 1f;

        // CATEGORY SETTINGS (cho CategoryProperty)
        [Header("Classification")]
        public ItemCategory category;

        [Tooltip("Các danh mục phụ (optional)")]
        public List<ItemCategory> secondaryCategories = new List<ItemCategory>();

        // TAG SETTINGS (cho TagProperty)
        [Header("Tag Settings")]
        [Tooltip("Tags đặc biệt (Quest, Rare, Cursed, Blessed, etc.)")]
        public List<ItemTag> tags = new List<ItemTag>();

        [Header("Category Settings")]
        [Tooltip("Danh mục chính của item")]
        public ItemCategory primaryCategory;

        // ECONOMY
        [Header("Economy")]
        [Tooltip("Giá mua từ shop")]
        public int baseValue = 0;

        [Tooltip("Giá bán cho shop")]
        public int sellValue = 0;

        [Tooltip("Item có thể được bán không")]
        public bool isSellable = true;

        [Tooltip("Item có thể mua không?")]
        public bool isPurchasable = true;

        // USAGE
        [Tooltip("Item có thể được tieu thu không")]
        public bool isUsable = true;

        [Tooltip("Item biến mất sau khi dùng? (Consumables)")]
        public bool isConsumable = false;

        [Tooltip("Thời gian cooldown sau khi dùng (giây)")]
        [Min(0)]
        public float cooldown = 0f;

        // DROP
        [Header("Other")]
        [Tooltip("Item có thể bị vứt bỏ không")]
        public bool canBeDropped = true;

        // UP LEVEL
        [Tooltip("Item có thể được nang cap không")]
        public bool canBeEquippable = true;

        // BASIC INFO
        public string ItemID => itemID;
        public string ItemName => itemName;
        public string Description => description;
        public Sprite Icon => icon;
        public MaterialTier MaterialTier => materialTier;

        // Stackable
        public bool IsStackable => isStackable;
        public int MaxStackSize => maxStackSize;

        // Durability
        public bool HasDurability => hasDurability;
        public float MaxDurability => maxDurability;
        public float DurabilityLossPerUse => durabilityLossPerUse;

        // Category
        public ItemCategory PrimaryCategory => primaryCategory;
        public IReadOnlyList<ItemCategory> SecondaryCategories => secondaryCategories;

        // Tags
        public IReadOnlyList<ItemTag> Tags => tags;

        // Economy
        public int BaseValue => baseValue;
        public int SellValue => sellValue > 0 ? sellValue : baseValue / 2;
        public bool IsSellable => isSellable;
        public bool IsPurchasable => isPurchasable;

        // Usage
        public bool IsUsable => isUsable;
        public bool IsConsumable => isConsumable;
        public float Cooldown => cooldown;

        // VALIDATION
        protected virtual void OnValidate()
        {
            this.ValidateItemData();
        }
        protected virtual void ValidateItemData()
        {
            // Auto-generate ID from filename
            if (string.IsNullOrEmpty(itemID))
            {
                itemID = name.Replace(" ", "_").ToLower();
            }

            if (maxStackSize < 1)
                maxStackSize = 1;

            if (!isStackable)
                maxStackSize = 1;

            // Validate durability
            if (maxDurability < 0)
                maxDurability = 0;

            if (!hasDurability)
            {
                maxDurability = 0;
                durabilityLossPerUse = 0;
            }

            // Logic checks
            if (isStackable && hasDurability)
            {
                Debug.LogWarning($"{itemName}: Stackable items with durability might cause confusion. " +
                    "Consider making it non-stackable or removing durability.", this);
            }

            if (isConsumable && hasDurability)
            {
                Debug.LogWarning($"{itemName}: Consumable items shouldn't have durability. " +
                    "They disappear after use.", this);
            }

            if (baseValue < 0)
                baseValue = 0;

            if (sellValue < 0)
                sellValue = 0;

            if (cooldown < 0)
                cooldown = 0;

            if (string.IsNullOrEmpty(itemName))
                itemName = "Unnamed Item";

            if (icon == null)
                Debug.LogWarning("Item has no icon", this);
        }

        // Tạo một Item instance mới từ ItemData này
        public virtual Item CreateInstance()
        {
            return new Item(this);
        }

        // Tạo Item instance với số lượng custom
        public virtual Item CreateInstance(int initialStack)
        {
            return new Item(this, initialStack, maxDurability);
        }

        // Tạo Item instance với stack và durability custom
        public virtual Item CreateInstance(int initialStack, float initialDurability)
        {
            return new Item(this, initialStack, initialDurability);
        }

        // Logic khi sử dụng item
        // Override trong subclass để thêm custom behavior
        public virtual bool Use(Item item)
        {
            if (!IsUsable)
            {
                return false;
            }

            // Consumable sẽ tự động giảm stack trong Item.Use()
            return true;
        }

        // Kiểm tra có category không
        public bool HasCategory(ItemCategory category)
        {
            if (category == null) return false;
            if (primaryCategory == category) return true;
            return secondaryCategories != null && secondaryCategories.Contains(category);
        }

        // Kiểm tra có tag không
        public bool HasTag(ItemTag tag)
        {
            if (tag == null) return false;
            return tags != null && tags.Contains(tag);
        }

        // Lấy tooltip text cho UI
        public virtual string GetTooltipText()
        {
            string tooltip = $"<b>{itemName}</b>";

            if (!string.IsNullOrEmpty(description))
            {
                tooltip += $"\n\n{description}";
            }

            if (materialTier != MaterialTier.Common)
            {
                tooltip += $"\n\n<color={GetRarityColor()}>{materialTier}</color>";
            }

            if (hasDurability)
            {
                tooltip += $"\n\nDurability: {maxDurability}";
            }

            if (isStackable)
            {
                tooltip += $"\nMax Stack: {maxStackSize}";
            }

            if (baseValue > 0)
            {
                tooltip += $"\n\nValue: {baseValue}g";
                if (isSellable)
                {
                    tooltip += $"\nSell: {SellValue}g";
                }
            }

            return tooltip;
        }

        /// Lấy màu theo rarity
        private string GetRarityColor()
        {
            return materialTier switch
            {
                MaterialTier.Common => "white",
                MaterialTier.Rare => "blue",
                MaterialTier.Epic => "purple",
                MaterialTier.Legendary => "orange",
                _ => "white"
            };
        }
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
            if (!isStackable || other.isStackable) return false;
            return this.itemID == other.itemID;
        }

        /// Lấy thông tin hiển thị tooltip

        public override string ToString()
        {
            return $"{itemName} ({itemID})";
        }


    }
}
