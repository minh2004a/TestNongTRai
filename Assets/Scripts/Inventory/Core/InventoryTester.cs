using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using TinyFarm.Items.UI;
using UnityEngine;


[RequireComponent(typeof(InventoryManager))]
public class InventoryTester : MonoBehaviour
{
    [Header("Test Items")]
    public ItemData[] testItems;

    [Header("Test Settings")]
    public bool autoAddTestItems = true;
    public KeyCode addItemKey = KeyCode.T;
    public KeyCode removeItemKey = KeyCode.R;
    public KeyCode clearInventoryKey = KeyCode.C;
    public KeyCode debugItemsKey = KeyCode.D; // NEW: Debug key

    [Header("Inventory Toggle")]
    public InventoryUI inventoryUI;
    public KeyCode toggleInventoryKey = KeyCode.Tab;

    private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = GetComponent<InventoryManager>();

        // DEBUG: Print all test items
        DebugTestItems();

        if (autoAddTestItems && testItems.Length > 0)
        {
            AddTestItems();
        }
    }

    private void Update()
    {
        // Test controls
        if (Input.GetKeyDown(addItemKey))
        {
            AddRandomTestItem();
        }

        if (Input.GetKeyDown(removeItemKey))
        {
            RemoveRandomItem();
        }

        if (Input.GetKeyDown(clearInventoryKey))
        {
            inventoryManager.ClearAllSlots();
            Debug.Log("[TEST] Inventory cleared!");
        }

        if (Input.GetKeyDown(toggleInventoryKey))
            ToggleInventory();

        if (Input.GetKeyDown(debugItemsKey))
            DebugTestItems();
    }

    private void ToggleInventory()
    {
        if (inventoryUI == null)
        {
            Debug.LogWarning("[TEST] InventoryUI not assigned!");
            return;
        }

        bool newState = !inventoryUI.gameObject.activeSelf;
        inventoryUI.gameObject.SetActive(newState);

        Debug.Log($"[TEST] Inventory {(newState ? "Opened" : "Closed")} with [{toggleInventoryKey}]");
    }

    [ContextMenu("Debug Test Items")]
    public void DebugTestItems()
    {
        Debug.Log("=== TEST ITEMS DEBUG ===");
        Debug.Log($"Total test items: {testItems.Length}");

        for (int i = 0; i < testItems.Length; i++)
        {
            ItemData item = testItems[i];
            if (item == null)
            {
                Debug.LogWarning($"[{i}] NULL ITEM");
            }
            else
            {
                Debug.Log($"[{i}] Name: {item.itemName} | ID: '{item.itemID}' | Type: {item.itemType}");

                // Check if ID is empty
                if (string.IsNullOrEmpty(item.itemID))
                {
                    Debug.LogError($"  ⚠️ EMPTY ITEM ID for {item.itemName}!");
                }
            }
        }
        Debug.Log("========================");
    }

    [ContextMenu("Add Test Items")]
    public void AddTestItems()
    {
        Debug.Log("[TEST] Adding test items...");

        foreach (var itemData in testItems)
        {
            if (itemData != null)
            {
                // Check if itemID is valid
                if (string.IsNullOrEmpty(itemData.itemID))
                {
                    Debug.LogError($"[TEST] ❌ Cannot add {itemData.itemName} - EMPTY ITEM ID!");
                    continue;
                }

                int quantity = itemData.isStackable ? Random.Range(1, 10) : 1;
                bool success = inventoryManager.AddItem(itemData.itemID, quantity);

                if (success)
                {
                    Debug.Log($"[TEST] ✅ Added {quantity}x {itemData.itemName} (ID: {itemData.itemID})");
                }
                else
                {
                    Debug.LogError($"[TEST] ❌ Failed to add {itemData.itemName} (ID: {itemData.itemID})");
                }
            }
        }
    }

    [ContextMenu("Add Random Item")]
    public void AddRandomTestItem()
    {
        if (testItems.Length == 0)
        {
            Debug.LogWarning("[TEST] No test items assigned!");
            return;
        }

        ItemData randomItem = testItems[Random.Range(0, testItems.Length)];

        if (randomItem == null)
        {
            Debug.LogError("[TEST] Selected item is NULL!");
            return;
        }

        if (string.IsNullOrEmpty(randomItem.itemID))
        {
            Debug.LogError($"[TEST] ❌ Cannot add {randomItem.itemName} - EMPTY ITEM ID!");
            return;
        }

        int quantity = randomItem.isStackable ? Random.Range(1, 5) : 1;

        Debug.Log($"[TEST] Attempting to add {quantity}x {randomItem.itemName} (ID: '{randomItem.itemID}')");
        bool success = inventoryManager.AddItem(randomItem.itemID, quantity);

        if (success)
        {
            Debug.Log($"[TEST] ✅ Added {quantity}x {randomItem.itemName}");
        }
        else
        {
            Debug.LogError($"[TEST] ❌ Failed to add {randomItem.itemName}");
        }
    }

    [ContextMenu("Remove Random Item")]
    public void RemoveRandomItem()
    {
        var occupiedSlots = inventoryManager.GetOccupiedSlots();
        if (occupiedSlots.Count == 0)
        {
            Debug.LogWarning("[TEST] No items to remove!");
            return;
        }

        var randomSlot = occupiedSlots[Random.Range(0, occupiedSlots.Count)];
        string itemID = randomSlot.ItemID;

        inventoryManager.RemoveItem(itemID, 1);
        Debug.Log($"[TEST] ✅ Removed 1x {itemID}");
    }

}
