using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
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
        public string Name => itemData?.itemName;
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
                itemData.isStackable,
                itemData.maxStackSize,
                1
            );

            // Initialize Durability
            durability = new DurabilityProperty(
                itemData.hasDurability,
                itemData.maxDurability,
                itemData.destroyOnBreak,
                itemData.repairCostMultiplier
            );

            // Initialize Category
            if (itemData.primaryCategory != null)
            {
                category = new CategoryProperty(itemData.primaryCategory);

                if (itemData.secondaryCategories != null)
                {
                    foreach (var cat in itemData.secondaryCategories)
                    {
                        category.AddSecondaryCategory(cat);
                    }
                }
            }

            // Initialize Tags
            tags = new TagProperty();
            if (itemData.tags != null)
            {
                foreach (var tag in itemData.tags)
                {
                    tags.AddTag(tag);
                }
            }
            // Subscribe to property events
            SubscribeToPropertyEvents();
        }

        private void SubscribeToPropertyEvents()
        {
            if (durability != null)
            {
                durability.OnItemBroken += OnDurabilityBroken;
            }
        }

        private void OnDurabilityBroken()
        {
            if (durability.DestroyOnBreak)
            {
                Destroy();
            }
        }

        // Sử dụng item
        public virtual bool Use()
        {
            if (!CanUse)
            {
                return false;
            }

            // Gọi Use logic từ ItemData
            bool success = itemData.Use(this);

            if (success)
            {
                // Giảm durability hoặc stack
                if (HasDurability)
                {
                    durability.Damage(itemData.durabilityLossPerUse);
                }
                else if (IsStackable)
                {
                    stackable.RemoveFromStack(1);
                }

                OnItemUsed?.Invoke(this);
            }

            return success;
        }

        // Hủy item
        public virtual void Destroy()
        {
            OnItemDestroyed?.Invoke(this);

            // Cleanup
            if (durability != null)
            {
                durability.OnItemBroken -= OnDurabilityBroken;
            }
        }

        // Clone item (tạo bản copy mới)
        public Item Clone()
        {
            return new Item(this);
        }

        // Set custom data - dùng để lưu thông tin runtime đặc biệt
        public void SetCustomData(string key, object value)
        {
            if (customData.ContainsKey(key))
            {
                customData[key] = value;
            }
            else
            {
                customData.Add(key, value);
            }

            OnCustomDataChanged?.Invoke(this, key, value);
        }

        // Get custom data
        public T GetCustomData<T>(string key, T defaultValue = default)
        {
            if (customData.TryGetValue(key, out object value))
            {
                if (value is T typeValue)
                {
                    return typeValue;
                }
            }
            return defaultValue;
        }

        // Kiểm tra có custom data không
        public bool HasCustomData(string key)
        {
            return customData.ContainsKey(key);
        }

        // Remove custom data
        public bool RemoveCustomData(string key)
        {
            return customData.Remove(key);
        }

        // Kiểm tra 2 item có cùng loại không (same ItemData)
        public bool IsSameType(Item other)
        {
            if (other == null) return false;
            return this.itemData == other.itemData;
        }

        // Kiểm tra có thể stack với item khác không
        public bool CanStackWith(Item other)
        {
            if (!IsSameType(other)) return false;
            if (stackable == null || other.stackable == null ) return false;
            return stackable.CanMergeWith(other.stackable);
        }

        private string GenerateInstanceID()
        {
            return $"{itemData?.itemID ?? "unknown"}_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
        }

        /// <summary>
        /// Save item data to JSON-friendly format
        /// </summary>
        public ItemSaveData ToSaveData()
        {
            return new ItemSaveData
            {
                instanceID = this.instanceID,
                itemDataID = this.itemData?.itemID,
                currentStack = this.stackable?.CurrentStack ?? 1,
                currentDurability = this.durability?.CurrentDurability ?? 0,
                customData = new Dictionary<string, object>(this.customData)
            };
        }

        public override string ToString()
        {
            string stackInfo = IsStackable ? $" x{CurrentStack}" : "";
            string durabilityInfo = HasDurability ? $" ({durability.DurabilityPercent:F0}%)" : "";
            return $"{Name}{stackInfo}{durabilityInfo}";
        }
        /// <summary>
        /// Load item from save data
        /// </summary>
        public static Item FromSaveData(ItemSaveData saveData, ItemDatabase database)
        {
            if (database == null || string.IsNullOrEmpty(saveData.itemDataID))
            {
                Debug.LogError("Cannot load item - invalid save data or database");
                return null;
            }

            ItemData data = database.GetItem(saveData.itemDataID);
            if (data == null)
            {
                Debug.LogError($"Cannot find ItemData with ID: {saveData.itemDataID}");
                return null;
            }

            Item item = new Item(data, saveData.currentStack, saveData.currentDurability);
            item.instanceID = saveData.instanceID; // Restore original ID
            item.customData = new Dictionary<string, object>(saveData.customData);

            return item;
        }
    }
}

