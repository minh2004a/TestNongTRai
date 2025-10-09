using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    [Header("Test Settings")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private ItemData[] testItems; // Kéo các ItemData vào đây để test

    [Header("Auto Add on Start")]
    [SerializeField] private bool addItemsOnStart = true;

    private void Start()
    {
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
        }

        if (addItemsOnStart)
        {
            AddTestItems();
        }
    }

    private void Update()
    {
        // Phím tắt để test
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddRandomItem();
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            RemoveRandomItem();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearInventory();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            PrintInventory();
        }
    }

    /// <summary>
    /// Thêm các test items vào inventory
    /// </summary>
    private void AddTestItems()
    {
        if (testItems == null || testItems.Length == 0)
        {
            Debug.LogWarning("Chưa gán test items!");
            return;
        }

        foreach (var item in testItems)
        {
            if (item != null)
            {
                int quantity = item.isStackable ? Random.Range(1, 10) : 1;
                inventoryManager.AddItem(item, quantity);
            }
        }

        Debug.Log("Đã thêm test items vào inventory!");
    }

    /// <summary>
    /// Thêm ngẫu nhiên 1 item
    /// </summary>
    private void AddRandomItem()
    {
        if (testItems == null || testItems.Length == 0) return;

        ItemData randomItem = testItems[Random.Range(0, testItems.Length)];
        if (randomItem != null)
        {
            int quantity = randomItem.isStackable ? Random.Range(1, 5) : 1;
            bool success = inventoryManager.AddItem(randomItem, quantity);

            if (success)
                Debug.Log($"✓ Đã thêm {quantity}x {randomItem.itemName}");
            else
                Debug.Log($"✗ Không thể thêm {randomItem.itemName}");
        }
    }

    /// <summary>
    /// Xóa ngẫu nhiên 1 item
    /// </summary>
    private void RemoveRandomItem()
    {
        if (testItems == null || testItems.Length == 0) return;

        ItemData randomItem = testItems[Random.Range(0, testItems.Length)];
        if (randomItem != null)
        {
            bool success = inventoryManager.RemoveItem(randomItem, 1);

            if (success)
                Debug.Log($"✓ Đã xóa 1x {randomItem.itemName}");
            else
                Debug.Log($"✗ Không tìm thấy {randomItem.itemName} trong inventory");
        }
    }

    /// <summary>
    /// Clear inventory
    /// </summary>
    private void ClearInventory()
    {
        inventoryManager.ClearInventory();
        Debug.Log("Đã xóa toàn bộ inventory!");
    }

    /// <summary>
    /// In ra console tất cả items trong inventory
    /// </summary>
    private void PrintInventory()
    {
        var slots = inventoryManager.GetAllSlots();
        Debug.Log("=== INVENTORY ===");

        int itemCount = 0;
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty())
            {
                Debug.Log($"Slot {i}: {slots[i].itemData.itemName} x{slots[i].quantity}");
                itemCount++;
            }
        }

        if (itemCount == 0)
        {
            Debug.Log("Inventory trống!");
        }

        Debug.Log("================");
    }

    // Hiển thị hướng dẫn trong Inspector
    private void OnValidate()
    {
        Debug.Log(@"
        === INVENTORY TESTER ===
        Phím tắt:
        - [1]: Thêm item ngẫu nhiên
        - [2]: Xóa item ngẫu nhiên  
        - [C]: Clear inventory
        - [I]: In ra inventory hiện tại
        ========================
        ");
    }
}
