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

    [Header("Inventory Toggle")]
    public InventoryUI inventoryUI;
    public KeyCode toggleInventoryKey = KeyCode.Tab;

    private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = GetComponent<InventoryManager>();

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

    [ContextMenu("Add Test Items")]
    public void AddTestItems()
    {
        foreach (var itemData in testItems)
        {
            if (itemData != null)
            {
                int quantity = itemData.isStackable ? Random.Range(1, 10) : 1;
                inventoryManager.AddItem(itemData.itemID, quantity);
                Debug.Log($"[TEST] Added {quantity}x {itemData.itemName}");
            }
        }
    }

    [ContextMenu("Add Random Item")]
    public void AddRandomTestItem()
    {
        if (testItems.Length == 0) return;

        ItemData randomItem = testItems[Random.Range(0, testItems.Length)];
        int quantity = randomItem.isStackable ? Random.Range(1, 5) : 1;

        bool success = inventoryManager.AddItem(randomItem.itemID, quantity);

        if (success)
        {
            Debug.Log($"[TEST] ✅ Added {quantity}x {randomItem.itemName}");
        }
        else
        {
            Debug.LogWarning($"[TEST] ❌ Failed to add {randomItem.itemName}");
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
