using UnityEngine;
using TinyFarm.Items;
using TinyFarm.Items.UI;

public class InventoryTabTester : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private InventoryTabController tabController;

    [Header("Test Settings")]
    [SerializeField] private bool addTestItemsOnStart = true;

    private void Start()
    {
        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();

        if (tabController == null)
            tabController = FindObjectOfType<InventoryTabController>();

        if (addTestItemsOnStart)
        {
            AddTestItems();
        }
    }

    [ContextMenu("Add Test Items")]
    public void AddTestItems()
    {
        if (inventoryManager == null)
        {
            Debug.LogError("[TabTester] InventoryManager not found!");
            return;
        }

        Debug.Log("=== ADDING TEST ITEMS ===");

        // Add Crops
        inventoryManager.AddItem("crop_dau", 5);
        inventoryManager.AddItem("crop_potato", 3);
        Debug.Log("✅ Added Crops");

        // Add Equipment
        inventoryManager.AddItem("gold_armor", 1);
        inventoryManager.AddItem("gold_boots", 1);
        inventoryManager.AddItem("gold_gloves", 1);
        Debug.Log("✅ Added Equipment");

        // Add Seeds (nếu có)
        // inventoryManager.AddItem("seed_wheat", 10);
        // inventoryManager.AddItem("seed_carrot", 8);

        // Add Tools (nếu có)
        // inventoryManager.AddItem("tool_hoe", 1);
        // inventoryManager.AddItem("tool_axe", 1);

        Debug.Log("=== TEST ITEMS ADDED ===");
    }

    [ContextMenu("Test Tab Switching")]
    public void TestTabSwitching()
    {
        if (tabController == null)
        {
            Debug.LogError("[TabTester] TabController not found!");
            return;
        }

        Debug.Log("=== TESTING TAB SWITCHING ===");

        // Test switch to All
        Debug.Log("Switching to ALL tab...");
        tabController.SwitchToAll();
    }


    private void SwitchToSeed()
    {
        Debug.Log("Switching to SEED tab...");
        tabController.SwitchToSeed();

        Invoke(nameof(SwitchToTool), 1f);
    }

    private void SwitchToTool()
    {
        Debug.Log("Switching to TOOL tab...");
        tabController.SwitchToTool();

        Invoke(nameof(BackToAll), 1f);
    }

    private void BackToAll()
    {
        Debug.Log("Switching back to ALL tab...");
        tabController.SwitchToAll();
    }

    [ContextMenu("Debug Current Tab")]
    public void DebugCurrentTab()
    {
        var allInventories = FindObjectsOfType<FilteredInventoryUI>();

        Debug.Log($"=== FOUND {allInventories.Length} FILTERED INVENTORIES ===");

        foreach (var inv in allInventories)
        {
            bool isActive = inv.gameObject.activeSelf;
            Debug.Log($"{inv.gameObject.name} - Active: {isActive}");
        }
    }

    private void Update()
    {
        // Keyboard shortcuts for testing
        if (Input.GetKeyDown(KeyCode.F1))
            AddTestItems();

        if (Input.GetKeyDown(KeyCode.F2))
            TestTabSwitching();

        if (Input.GetKeyDown(KeyCode.F3))
            DebugCurrentTab();
    }
}