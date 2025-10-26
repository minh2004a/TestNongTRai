using System.Collections.Generic;
using System.Linq;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.Items.UI
{
    public class FilteredInventoryUI : MonoBehaviour
    {
        [Header("Filter Settings")]
        [SerializeField] private ItemType[] allowedTypes;
        [SerializeField] private bool showAllTypes = true;

        [Header("References")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotPrefab;

        [Header("UI Settings")]
        [SerializeField] private int maxSlotsToShow = 40;

        private List<SlotUI> slotUIs = new List<SlotUI>();
        private bool isInitialized = false;
        private bool needsRefresh = false; // Flag để track cần refresh

        private void Awake()
        {
            if (inventoryManager == null)
            {
                inventoryManager = FindObjectOfType<InventoryManager>();
            }
        }

        private void OnEnable()
        {
            if (!isInitialized)
            {
                Initialize();
            }

            // CRITICAL: Luôn refresh khi panel được bật
            RefreshDisplay();
            needsRefresh = false;
        }

        public void Initialize()
        {
            if (isInitialized)
            {
                return;
            }

            if (inventoryManager == null)
            {
                Debug.LogError("[FilteredInventoryUI] InventoryManager not found!");
                return;
            }

            CreateSlotUIs();

            // Subscribe to inventory events
            inventoryManager.OnInventoryChanged += OnInventoryChangedHandler;

            isInitialized = true;

        }

        private void OnInventoryChangedHandler()
        {
            if (gameObject.activeSelf)
            {
                // Panel đang active → refresh ngay
                RefreshDisplay();
            }
            else
            {
                // Panel đang inactive → đánh dấu cần refresh
                needsRefresh = true;
            }
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
                else
                {
                    Debug.LogError($"[FilteredInventoryUI] SlotPrefab missing SlotUI component!");
                }
            }

        }

        public void RefreshDisplay()
        {
            if (!isInitialized)
            {
                return;
            }

            if (inventoryManager == null)
            {
                return;
            }

            // Get filtered slots
            List<InventorySlot> filteredSlots = GetFilteredSlots();

            // Update ALL UI slots
            for (int i = 0; i < slotUIs.Count; i++)
            {
                if (slotUIs[i] == null)
                {
                    continue;
                }

                if (i < filteredSlots.Count)
                {
                    // Show slot with item
                    slotUIs[i].Initialize(filteredSlots[i]);
                    slotUIs[i].gameObject.SetActive(true);
                }
                else
                {
                    // Create empty slot
                    InventorySlot emptySlot = new InventorySlot(i, SlotType.Normal);
                    slotUIs[i].Initialize(emptySlot);
                    slotUIs[i].gameObject.SetActive(true);
                }
            }
        }

        private List<InventorySlot> GetFilteredSlots()
        {
            if (inventoryManager == null)
            {
                return new List<InventorySlot>();
            }

            var allSlots = inventoryManager.GetAllSlots();

            if (allSlots == null)
            {
                return new List<InventorySlot>();
            }

            if (showAllTypes)
            {
                // Tab "All" - show ALL non-empty slots
                return allSlots.Where(slot => !slot.IsEmpty).ToList();
            }
            else
            {
                // Filtered tabs
                return allSlots.Where(slot =>
                {
                    if (slot.IsEmpty) return false;

                    ItemType itemType = slot.Item.ItemData.GetItemType();
                    return allowedTypes.Contains(itemType);
                }).ToList();
            }
        }

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
        }

        private void OnDestroy()
        {

            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged -= OnInventoryChangedHandler;
            }
        }

        // Debug helper
        [ContextMenu("Force Refresh")]
        public void ForceRefresh()
        {
            RefreshDisplay();
        }
    }
}

