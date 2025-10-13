using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TinyFarm.Items;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    private static ItemManager instance;
    public static ItemManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ItemManager>();
                if (instance == null)
                {
                    GameObject go = new GameObject("ItemManager");
                    instance = go.AddComponent<ItemManager>();
                }
            }
            return instance;
        }
    }

    [Header("Configuration")]
    [SerializeField] private ItemDatabase itemDatabase;
    [SerializeField] private bool validateOnCreate = true;
    [SerializeField] private bool trackAllItems = true;

    // Factory
    private ItemFactory itemFactory;
    public ItemFactory Factory => itemFactory;

    // Tracking
    private Dictionary<string, Item> trackedItems = new Dictionary<string, Item>();
    private List<Item> allItems = new List<Item>();

    // Events
    public event Action<Item> OnItemCreated;
    public event Action<Item> OnItemDestroyed;

    // Properties
    public ItemDatabase Database => itemDatabase;
    public int TrackedItemCount => trackedItems.Count;
    public IReadOnlyList<Item> AllItems => allItems;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        Initialize();
    }

    private void Initialize()
    {
        if (itemDatabase == null)
        {
            Debug.LogError("ItemManager: No ItemDatabase assigned!");
            return;
        }

        itemFactory = new ItemFactory(itemDatabase, validateOnCreate);
        Debug.Log("ItemManager initialized");
    }

    /// Tạo item từ ID
    public Item CreateItem(string itemID)
    {
        Item item = itemFactory.CreateItem(itemID);

        if (item != null)
        {
            RegisterItem(item);
        }

        return item;
    }

    // Tạo item với quantity
    public Item CreateItem(string itemID, int quantity)
    {
        Item item = itemFactory.CreateItem(itemID, quantity);

        if (item != null)
        {
            RegisterItem(item);
        }

        return item;
    }

    // Tạo item với durability
    public Item CreateItem(string itemID, int quantity, float durability)
    {
        Item item = itemFactory.CreateItem(itemID, quantity, durability);

        if (item != null)
        {
            RegisterItem(item);
        }

        return item;
    }

    // Clone item
    public Item CloneItem(Item original)
    {
        Item clone = itemFactory.CloneItem(original);

        if (clone != null)
        {
            RegisterItem(clone);
        }

        return clone;
    }

    private void RegisterItem(Item item)
    {
        if (item == null) return;

        if (trackAllItems)
        {
            if (!trackedItems.ContainsKey(item.InstanceID))
            {
                trackedItems.Add(item.InstanceID, item);
                allItems.Add(item);

                // Subscribe to events
                item.OnItemDestroyed += OnItemDestroyedHandler;
            }
        }

        OnItemCreated?.Invoke(item);
    }

    private void UnregisterItem(Item item)
    {
        if (item == null) return;

        if (trackedItems.ContainsKey(item.InstanceID))
        {
            trackedItems.Remove(item.InstanceID);
            allItems.Remove(item);

            item.OnItemDestroyed -= OnItemDestroyedHandler;
        }
    }

    private void OnItemDestroyedHandler(Item item)
    {
        UnregisterItem(item);
        OnItemDestroyed?.Invoke(item);
    }

    // Lấy item theo instance ID
    public Item GetTrackedItem(string instanceID)
    {
        trackedItems.TryGetValue(instanceID, out Item item);
        return item;
    }

    // Lấy tất cả items theo ItemData ID
    public List<Item> GetItemsByID(string itemID)
    {
        return allItems.FindAll(item => item.ID == itemID);
    }

    // Lấy tất cả items theo category
    public List<Item> GetItemsByCategory(ItemCategory category)
    {
        return allItems.FindAll(item =>
            item.Category != null && item.Category.HasCategory(category)
        );
    }

    // Lấy tất cả items theo tag
    public List<Item> GetItemsByTag(ItemTag tag)
    {
        return allItems.FindAll(item =>
            item.Tags != null && item.Tags.HasTag(tag)
        );
    }

    /// Validate item
    public ItemValidator.ValidationResult ValidateItemData(Item item)
    {
        return ItemValidator.ValidationResult.ValidateItemData(item);
    }

    /// Validate tất cả tracked items
    public void ValidateAllItems()
    {
        Debug.Log("=== Validating All Items ===");

        int validCount = 0;
        int invalidCount = 0;

        foreach (var item in allItems)
        {
            var result = ItemValidator.ValidationResult.ValidateItemData(item);

            if (result.IsValid)
            {
                validCount++;
            }
            else
            {
                invalidCount++;
                Debug.LogWarning($"Invalid item: {item.Name}\n{result.GetReport()}");
            }
        }

        Debug.Log($"Validation complete: {validCount} valid, {invalidCount} invalid");
    }

    /// Lấy thống kê items
    public string GetStatistics()
    {
        int totalItems = allItems.Count;
        int stackableItems = allItems.Count(i => i.IsStackable);
        int durabilityItems = allItems.Count(i => i.HasDurability);
        int brokenItems = allItems.Count(i => i.IsBroken);

        return $"Total Items: {totalItems}\n" +
               $"Stackable: {stackableItems}\n" +
               $"With Durability: {durabilityItems}\n" +
               $"Broken: {brokenItems}";
    }

    // Clear tất cả tracked items
    public void ClearAllItems()
    {
        foreach (var item in allItems.ToList())
        {
            item.Destroy();
        }

        trackedItems.Clear();
        allItems.Clear();

        Debug.Log("All items cleared");
    }

    // Debug: In ra tất cả items
    [ContextMenu("Debug: Print All Items")]
    public void DebugPrintAllItems()
    {
        Debug.Log("=== All Tracked Items ===");
        foreach (var item in allItems)
        {
            Debug.Log($"  {item}");
        }
        Debug.Log($"Total: {allItems.Count} items");
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
