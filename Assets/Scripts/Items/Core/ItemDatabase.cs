using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TinyFarm.Items
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Game/Item/Item Database")]
    public class ItemDatabase : ScriptableObject
    {
        [Header("Database Settings")]
        [Tooltip("Tự động load tất cả ItemData khi khởi tạo")]
        public bool autoLoadAllItems = true;

        [Header("Item Collection")]
        [Tooltip("Danh sách tất cả items trong game")]
        public List<ItemData> items = new List<ItemData>();

        // Cache dictionaries để truy xuất nhanh
        private Dictionary<string, ItemData> itemDictionary;
        private Dictionary<ItemType, ItemData> itemByType;
        private bool isInitialized = false;

        public Dictionary<ItemType, List<ItemData>> itemsByType = new Dictionary<ItemType, List<ItemData>>();

        // Khởi tạo database và cache data
        public void Initialize()
        {
            if (isInitialized) return;

            LoadAllItems();
            BuildCacheDictionary();
            isInitialized = true;
        }

        // Load tất cả ItemData từ Resources (nếu autoLoad = true)
        public void LoadAllItems()
        {
            if (!autoLoadAllItems) return;

            // Load tất cả ItemData từ thư mục Resources/Data/Items
            ItemData[] loadedItems = Resources.LoadAll<ItemData>("Data/Items");

            foreach (var item in loadedItems)
            {
                if (!items.Contains(item))
                {
                    items.Add(item);
                }
            }
        }

        // Xây dựng cache dictionaries để truy xuất nhanh
        private void BuildCacheDictionary()
        {
            // Dictionary theo itemID
            itemDictionary = new Dictionary<string, ItemData>();
            foreach (var item in items)
            {
                if (!string.IsNullOrEmpty(item.itemID))
                {
                    if (itemDictionary.ContainsKey(item.itemID))
                    {
                        Debug.LogWarning($"[ItemDatabase] Duplicate itemID found: {item.itemID}");
                    }
                    else
                    {
                        itemDictionary[item.itemID] = item;
                    }
                }
            }

            // Dictionary theo ItemType
            Dictionary<ItemType, List<ItemData>> itemsByType = new Dictionary<ItemType, List<ItemData>>();
            foreach (ItemType type in System.Enum.GetValues(typeof(ItemType)))
            {
                List<ItemData> itemsOfType = new List<ItemData>();
                foreach (var item in items)
                {
                    if (item != null && item.itemType == type)
                    {
                        itemsOfType.Add(item);
                    }
                }
                itemsByType[type] = itemsOfType;
            }
        }

        // Lấy ItemData theo itemID
        public ItemData GetItem(string itemID)
        {
            if (!isInitialized) Initialize();

            if (itemDictionary.TryGetValue(itemID, out ItemData item)) return item;

            return null;
        }

        // Lấy ItemData theo itemID với type cast
        public T GetItem<T>(string itemID) where T : ItemData
        {
            ItemData item = GetItem(itemID);
            if (item != null && item is T)
            {
                return item as T;
            }

            return null;
        }

        // Lấy danh sách items theo ItemType
        public List<ItemData> GetItemsByType(ItemType type)
        {
            if (!isInitialized) Initialize();

            if (itemsByType.TryGetValue(type, out List<ItemData> itemList))
            {
                return new List<ItemData>(itemList);
            }

            return new List<ItemData>();
        }

        // Lấy danh sách items theo ItemType với type cast
        public List<T> GetItemsByType<T>(ItemType type) where T : ItemData
        {
            List<ItemData> items = GetItemsByType(type);
            return items.OfType<T>().ToList();
        }

        // Kiểm tra xem itemID có tồn tại không
        public bool ItemExists(string itemID)
        {
            if (!isInitialized) Initialize();

            return itemDictionary.ContainsKey(itemID);
        }

        // Lấy tất cả items trong database
        public List<ItemData> GetAllItems()
        {
            if (!isInitialized) Initialize();

            return new List<ItemData>(items);
        }

        // Lọc items có thể stack
        public List<ItemData> GetStackableItems()
        {
            if (!isInitialized) Initialize();

            return items.Where(i => i.canBeStacked).ToList();
        }

        // Lọc items có thể tiêu thụ
        public List<ItemData> GetConsumableItems()
        {
            if (!isInitialized) Initialize();

            return items.Where(i => i.canBeConsumable).ToList();
        }

        // Lọc items có thể trang bị
        public List<ItemData> GetisEquippableItems()
        {
            if (!isInitialized) Initialize();

            return items.Where(i => i.canBeEquippable).ToList();
        }

        // Refresh database - dùng trong Editor
        [ContextMenu("Refresh Database")]
        public void RefreshDatabase()
        {
            isInitialized = false;
            items.Clear();
            Initialize();
            Debug.Log("[ItemDatabase] Database refreshed!");
        }

        // Validate database - kiểm tra duplicate IDs
        [ContextMenu("Validate Database")]
        public void ValidateDatabase()
        {
            HashSet<string> seenIDs = new HashSet<string>();
            List<string> duplicates = new List<string>();

            foreach (var item in items)
            {
                if (string.IsNullOrEmpty(item.itemID))
                {
                    continue;
                }
                if (seenIDs.Contains(item.itemID))
                {
                    duplicates.Add(item.itemID);
                }
                else
                {
                    seenIDs.Add(item.itemID);
                }
            }

            if (duplicates.Count > 0)
            {
                Debug.LogError($"[ItemDatabase] Found {duplicates.Count} duplicate IDs: {string.Join(", ", duplicates)}");
            }
            else
            {
                Debug.Log("[ItemDatabase] Validation passed! No duplicate IDs found.");
            }
        }


        // In ra thống kê database
        [ContextMenu("Print Statistics")]
        public void PrintStatistics()
        {
            if (!isInitialized) Initialize();

            foreach(ItemType type in System.Enum.GetValues(typeof(ItemType)))
            {
                int count = GetItemsByType(type).Count;
                if (count > 0)
                {
                    Debug.Log($"  - {type}: {count}");
                }
            }
        }
        private void OnEnable()
        {
            // Auto-initialize khi ScriptableObject được load
            if (autoLoadAllItems)
            {
                Initialize();
            }
        }
    }
}

