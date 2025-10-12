using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    // Đại diện cho một instance của item trong game
    // Mỗi item có unique ID và state riêng
    [Serializable]
    public class Item
    {
        // CORE DATA
        [SerializeField] private string instanceID;
        [SerializeField] private ItemData itemData;

        // Properties
        [SerializeField] private StackableProperty stackable;
        [SerializeField] private DurabilityProperty durability;
        [SerializeField] private CategoryProperty category;
        [SerializeField] private TagProperty tags;

        // Properties
        public StackableProperty Stackable => stackable;
        public DurabilityProperty Durability => durability;
        public CategoryProperty Category => category;
        public TagProperty Tags => tags;

        // RUN DATA
        [SerializeField] private Dictionary<string, object> customData = new Dictionary<string, object>();

        // Events
        public event Action<Item> OnItemUsed;
        public event Action<Item> OnItemDestroyed;
        public event Action<Item, string, object> OnCustomDataChanged;

        // PROPERTIES ACCESSORS
        public string InstanceID => instanceID;
        public ItemData ItemData => itemData;
        public string ID => itemData?.itemID;
        public string name => itemData?.itemName;
        public string Description => itemData?.description;
        public Sprite Icon => itemData?.icon;
        public MaterialTier materialTier => itemData?.materialTier ?? MaterialTier.Common;

        // Quick checks
        public bool IsStackable => stackable?.IsStackable ?? false;
        public bool HasDurability => durability?.HasDurability ?? false;
        public int CurrentStack => stackable?.CurrentStack ?? 1;
        public bool IsBroken => durability?.IsBroken ?? false;
        public bool CanUse => !HasDurability || durability.CanUse();

        // Constructor chính - tạo item từ ItemData
        public Item(ItemData data)
        {
            if (data == null)
            {
                return;
            }

            this.instanceID = GenerateInstanceID();
            this.itemData = data;

            // Initialize properties from ItemData
            InitializeProperties();
        }

        public Item(ItemData data, int initialStack, float initialDurability) : this(data)
        {
            if (stackable != null && stackable.IsStackable)
            {
                stackable.SetStack(initialStack);
            }

            if (durability != null && durability.HasDurability)
            {
                durability.SetDurability(initialDurability);
            }
        }

        // Copy constructor - tạo bản copy của item khác
        public Item(Item other)
        {
            if (other == null)
            {
                return;
            }

            this.instanceID = GenerateInstanceID();
            this.itemData = other.itemData;

            // Clone properties
            this.stackable = other.stackable?.Clone();
            this.durability = other.durability?.Clone();
            this.category = other.category?.Clone();
            this.tags = other.tags?.Clone();

            // Copy custom data
            this.customData = new Dictionary<string, object>(other.customData);
        }

        private void InitializeProperties()
        {
            // Initialize Stackable
            stackable = new StackableProperty(
                itemData.canBeStacked,
                itemData.maxStack,
                1
            );

            // Initialize Durability
            durability = new DurabilityProperty(
                itemData.hasDurability,
                itemData.maxDurability,
                itemData.destroyOnBreak,
                itemData.repairCostMultiplier
            );

        }

        private string GenerateInstanceID()
        {
            return $"{itemData?.itemID ?? "unknown"}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }
    }
}

