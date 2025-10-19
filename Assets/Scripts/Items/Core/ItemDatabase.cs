using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

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
        private bool isInitialized = false;

        public Dictionary<ItemType, List<ItemData>> itemsByType = new Dictionary<ItemType, List<ItemData>>();

        // Khởi tạo database và cache data

        private void OnEnable()
        {
            // Auto-initialize khi ScriptableObject được load
            if (autoLoadAllItems)
            {
                Initialize();
            }
        }
        public void Initialize()
        {
            if (isInitialized && itemDictionary != null && itemDictionary.Count > 0)
            {
                return;
            }

            // Load items trước
            LoadAllItems();

            // Build cache sau
            BuildCacheDictionary();

            // CHỈ set flag SAU KHI hoàn thành THÀNH CÔNG
            if (itemDictionary != null && itemDictionary.Count > 0)
            {
                isInitialized = true;
            }
            else
            {
                isInitialized = false;
            }
        }

        // Load tất cả ItemData từ Resources (nếu autoLoad = true)
        public void LoadAllItems()
        {
            // Nếu items list đã có data từ Inspector, sử dụng nó
            if (items != null && items.Count > 0)
            {
                // Validate items
                for (int i = items.Count - 1; i >= 0; i--)
                {
                    if (items[i] == null)
                    {
                        items.RemoveAt(i);
                    }
                    else if (string.IsNullOrEmpty(items[i].itemID))
                    {
                        Debug.LogWarning($"[ItemDatabase] Item '{items[i].name}' has empty itemID!");
                    }
                }
                return;
            }

            // Nếu không có items và autoLoad = false, skip
            if (!autoLoadAllItems)
            {
                return;
            }

            // Load từ Resources với nhiều paths

            List<ItemData> loadedItems = new List<ItemData>();

            // Try multiple paths
            string[] possiblePaths = new string[]
            {
                "Data/Items",
                "Data/Farming/Items",
                "Items"
            };

            foreach (string path in possiblePaths)
            {
                ItemData[] foundItems = Resources.LoadAll<ItemData>(path);
                if (foundItems.Length > 0)
                {
                    loadedItems.AddRange(foundItems);
                }
            }

            Debug.Log($"[ItemDatabase] Total items found: {loadedItems.Count}");

            if (loadedItems.Count == 0)
            {
                return;
            }

            // Clear và load items từ Resources
            items.Clear();

            foreach (var item in loadedItems)
            {
                if (item == null)
                {
                    Debug.LogWarning("[ItemDatabase] Null item found in Resources!");
                    continue;
                }

                if (string.IsNullOrEmpty(item.itemID))
                {
                    Debug.LogWarning($"[ItemDatabase] Item '{item.name}' has empty itemID! Skipping...");
                    continue;
                }

                // Check for duplicates
                if (items.Any(i => i.itemID == item.itemID))
                {
                    Debug.LogWarning($"[ItemDatabase] Duplicate itemID '{item.itemID}' found, skipping...");
                    continue;
                }

                items.Add(item);
                Debug.Log($"[ItemDatabase] Loaded item: {item.itemID} ({item.itemName})");
            }

            Debug.Log($"[ItemDatabase] Total items loaded: {items.Count}");
        }

        // Xây dựng cache dictionaries để truy xuất nhanh
        private void BuildCacheDictionary()
        {
            // Initialize dictionary
            itemDictionary = new Dictionary<string, ItemData>();

            int validCount = 0;
            int nullCount = 0;
            int emptyIDCount = 0;

            foreach (var item in items)
            {
                if (item == null)
                {
                    nullCount++;
                    Debug.LogWarning("[ItemDatabase] Null item in items list!");
                    continue;
                }

                if (string.IsNullOrEmpty(item.itemID))
                {
                    emptyIDCount++;
                    Debug.LogWarning($"[ItemDatabase] Item '{item.name}' has no itemID!");
                    continue;
                }

                if (itemDictionary.ContainsKey(item.itemID))
                {
                    Debug.LogWarning($"[ItemDatabase] Duplicate itemID found: {item.itemID}");
                }
                else
                {
                    itemDictionary[item.itemID] = item;
                    validCount++;
                }
            }

            // Dictionary theo ItemType
            itemsByType = new Dictionary<ItemType, List<ItemData>>();
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
        public ItemData GetItemByID(string itemID)
        {
            // Ensure initialized
            if (!isInitialized || itemDictionary == null || itemDictionary.Count == 0)
            {
                Initialize();
            }

            // Double check after initialize
            if (itemDictionary == null)
            {
                // Last resort: build directly
                BuildCacheDictionary();

                if (itemDictionary == null)
                {
                    Debug.LogError("[ItemDatabase] ❌ STILL NULL after BuildCacheDictionary!");
                    return null;
                }
            }

            if (itemDictionary.TryGetValue(itemID, out ItemData item))
            {
                return item;
            }
            else
            {
                // Show first 10 available IDs
                var availableIDs = itemDictionary.Keys.Take(10).ToList();
                Debug.LogError($"[ItemDatabase] Sample available IDs: {string.Join(", ", availableIDs)}");

                return null;
            }
        }

        // Lấy ItemData theo itemID với type cast
        public T GetItem<T>(string itemID) where T : ItemData
        {
            ItemData item = GetItemByID(itemID);
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

            return items.Where(i => i.isStackable).ToList();
        }

        // Lọc items có thể tiêu thụ
        public List<ItemData> GetConsumableItems()
        {
            if (!isInitialized) Initialize();

            return items.Where(i => i.isUsable).ToList();
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
        
    }
}

