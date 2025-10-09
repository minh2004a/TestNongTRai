using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

public class InventoryTester : MonoBehaviour
{
    [Header("Test Items")]
    [SerializeField] private ItemData testItem1; // Kéo thả ItemData vào đây
    [SerializeField] private ItemData testItem2;

    [Header("Test Settings")]
    [SerializeField] private bool testOnStart = true;

    void Start()
    {
        if (testOnStart)
        {
            // Đợi 0.5s để đảm bảo InventoryManager đã khởi tạoo
            Invoke("RunTests", 0.5f);
        }
    }

    void Update()
    {
        // Phím tắt để test trong game
        if (Input.GetKeyDown(KeyCode.T))
        {
            RunTests();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            InventoryManager.Instance.PrintInventory();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            InventoryManager.Instance.ClearInventory();
        }
    }

    public void RunTests()
    {
        Debug.Log("===== STARTING INVENTORY TESTS =====");

        if (testItem1 == null)
        {
            Debug.LogError("Test Item 1 is not assigned!");
            return;
        }

        // Test 1: Thêm item
        Debug.Log("\n--- Test 1: Add Items ---");
        bool result1 = InventoryManager.Instance.AddItem(testItem1, 5);
        Debug.Log($"Add 5x {testItem1.itemName}: {result1}");

        // Test 2: Thêm thêm item (test stacking)
        Debug.Log("\n--- Test 2: Add More Items (Stacking) ---");
        bool result2 = InventoryManager.Instance.AddItem(testItem1, 3);
        Debug.Log($"Add 3x {testItem1.itemName}: {result2}");

        // Test 3: Kiểm tra có item không
        Debug.Log("\n--- Test 3: Check Item Exists ---");
        bool hasItem = InventoryManager.Instance.HasItem(testItem1, 5);
        Debug.Log($"Has 5x {testItem1.itemName}: {hasItem}");

        // Test 4: Đếm số lượng item
        Debug.Log("\n--- Test 4: Count Items ---");
        int count = InventoryManager.Instance.GetItemCount(testItem1);
        Debug.Log($"Total {testItem1.itemName}: {count}");

        // Test 5: Xóa item
        Debug.Log("\n--- Test 5: Remove Items ---");
        bool result3 = InventoryManager.Instance.RemoveItem(testItem1, 4);
        Debug.Log($"Remove 4x {testItem1.itemName}: {result3}");

        int countAfter = InventoryManager.Instance.GetItemCount(testItem1);
        Debug.Log($"Remaining {testItem1.itemName}: {countAfter}");

        // Test 6: Thêm item khác (nếu có)
        if (testItem2 != null)
        {
            Debug.Log("\n--- Test 6: Add Different Item ---");
            bool result4 = InventoryManager.Instance.AddItem(testItem2, 10);
            Debug.Log($"Add 10x {testItem2.itemName}: {result4}");
        }

        // Test 7: In toàn bộ inventory
        Debug.Log("\n--- Test 7: Print Full Inventory ---");
        InventoryManager.Instance.PrintInventory();

        // Test 8: Test max stack size
        Debug.Log("\n--- Test 8: Test Max Stack Size ---");
        int maxStack = testItem1.maxStack;
        bool result5 = InventoryManager.Instance.AddItem(testItem1, maxStack);
        Debug.Log($"Add {maxStack}x {testItem1.itemName}: {result5}");

        // Test 9: Thử xóa nhiều hơn số có
        Debug.Log("\n--- Test 9: Try Remove More Than Available ---");
        bool result6 = InventoryManager.Instance.RemoveItem(testItem1, 999);
        Debug.Log($"Remove 999x {testItem1.itemName}: {result6} (Should fail)");

        Debug.Log("\n===== INVENTORY TESTS COMPLETED =====");
        InventoryManager.Instance.PrintInventory();
    }

    // Test thêm item qua Inspector button
    [ContextMenu("Add Test Item 1 (x5)")]
    public void AddTestItem1()
    {
        if (testItem1 != null)
        {
            InventoryManager.Instance.AddItem(testItem1, 5);
            InventoryManager.Instance.PrintInventory();
        }
    }

    [ContextMenu("Add Test Item 2 (x10)")]
    public void AddTestItem2()
    {
        if (testItem2 != null)
        {
            InventoryManager.Instance.AddItem(testItem2, 10);
            InventoryManager.Instance.PrintInventory();
        }
    }

    [ContextMenu("Clear Inventory")]
    public void ClearInventory()
    {
        InventoryManager.Instance.ClearInventory();
    }

    [ContextMenu("Print Inventory")]
    public void PrintInventory()
    {
        InventoryManager.Instance.PrintInventory();
    }
}
