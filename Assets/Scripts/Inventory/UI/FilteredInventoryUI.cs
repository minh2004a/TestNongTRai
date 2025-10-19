using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TinyFarm.Items;

namespace TinyFarm.Items.UI
{
    public class FilteredInventoryUI : MonoBehaviour
    {
        [Header("Filter Settings")]
        [SerializeField] private ItemType[] allowedTypes;
        [SerializeField] private bool showAllTypes = true; // Cho tab "All"

        [Header("References")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;

        [Header("UI Settings")]
        [SerializeField] private int maxSlotsToShow = 40;

        private List<SlotUI> slotUIs = new List<SlotUI>();
        private bool isInitialized = false;

        private void Awake()
        {
            if (inventoryManager == null)
            {
                inventoryManager = FindObjectOfType<InventoryManager>();
            }
        }

        private void OnEnable()
        {
            // Refresh khi panel được bật
            if (isInitialized)
            {
                RefreshDisplay();
            }
            else
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            if (isInitialized) return;

            if (inventoryManager == null)
            {
                Debug.LogError("[FilteredInventoryUI] InventoryManager not found!");
                return;
            }

            CreateSlotUIs();

            // Subscribe to inventory events
            inventoryManager.OnInventoryChanged += RefreshDisplay;

            isInitialized = true;
            RefreshDisplay();

            Debug.Log($"[FilteredInventoryUI] Initialized - ShowAll: {showAllTypes}, Types: {string.Join(", ", allowedTypes)}");
        }

        private void CreateSlotUIs()
        {
            // Clear existing slots
            foreach (var slot in slotUIs)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            slotUIs.Clear();

            if (slotsContainer == null || slotPrefab == null)
            {
                Debug.LogError("[FilteredInventoryUI] SlotsContainer or SlotPrefab not assigned!");
                return;
            }

            // Create slot UIs
            for (int i = 0; i < maxSlotsToShow; i++)
            {
                GameObject slotObj = Instantiate(slotPrefab, slotsContainer);
                SlotUI slotUI = slotObj.GetComponent<SlotUI>();

                if (slotUI != null)
                {
                    slotUIs.Add(slotUI);
                }
            }

            Debug.Log($"[FilteredInventoryUI] Created {slotUIs.Count} slot UIs");
        }

        public void RefreshDisplay()
        {
            if (!isInitialized || inventoryManager == null) return;

            // Get filtered slots from inventory
            List<InventorySlot> filteredSlots = GetFilteredSlots();

            // Update UI slots
            for (int i = 0; i < slotUIs.Count; i++)
            {
                if (i < filteredSlots.Count)
                {
                    // Show slot with item - Dùng Initialize() thay vì SetSlot()
                    slotUIs[i].Initialize(filteredSlots[i]);
                    slotUIs[i].gameObject.SetActive(true);
                }
                else
                {
                    // Hide unused slot - Initialize với empty slot
                    InventorySlot emptySlot = new InventorySlot(i, SlotType.Normal);
                    slotUIs[i].Initialize(emptySlot);
                    slotUIs[i].gameObject.SetActive(true); // Hoặc false nếu muốn ẩn
                }
            }

            Debug.Log($"[FilteredInventoryUI] Refreshed display with {filteredSlots.Count} items");
        }

        private List<InventorySlot> GetFilteredSlots()
        {
            var allSlots = inventoryManager.GetAllSlots();

            if (showAllTypes)
            {
                // Show tất cả items (non-empty slots)
                var result = allSlots.Where(slot => !slot.IsEmpty).ToList();
                Debug.Log($"[FilteredInventoryUI] Show all: {result.Count} non-empty slots");
                return result;
            }
            else
            {
                // Filter theo allowed types
                var result = allSlots.Where(slot =>
                {
                    if (slot.IsEmpty) return false;

                    ItemType itemType = slot.Item.ItemData.GetItemType();
                    bool allowed = allowedTypes.Contains(itemType);

                    if (allowed)
                    {
                        Debug.Log($"[FilteredInventoryUI] ✅ {slot.ItemName} ({itemType}) - ALLOWED");
                    }

                    return allowed;
                }).ToList();

                Debug.Log($"[FilteredInventoryUI] Filtered by {string.Join(", ", allowedTypes)}: {result.Count} items");
                return result;
            }
        }

        // Public method để set filter từ bên ngoài
        public void SetFilter(ItemType[] types)
        {
            allowedTypes = types;
            showAllTypes = false;
            RefreshDisplay();
        }

        public void ShowAllTypes()
        {
            showAllTypes = true;
            RefreshDisplay();
        }

        private void OnDisable()
        {
            // Optional: Có thể để trống hoặc clear
        }

        private void OnDestroy()
        {
            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged -= RefreshDisplay;
            }
        }
    }
}

