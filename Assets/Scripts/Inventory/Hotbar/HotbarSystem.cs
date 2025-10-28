using System;
using System.Collections;
using System.Collections.Generic;
using TinyFarm.PlayerInput;      // ✅ THÊM
using TinyFarm.Tools;      // ✅ THÊM
using UnityEngine;

namespace TinyFarm.Items.UI 
{
    public class HotbarSystem : MonoBehaviour
    {
        [Header("Hotbar Settings")]
        [SerializeField] private int hotbarSize = 10;
        [SerializeField] private int startSlotIndex = 0;
        [SerializeField] private bool allowHotbarSwap = true;
        [SerializeField] private HotBarUI hotbarUI;

        [Header("References")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private PlayerInputHandler inputHandler;
        [SerializeField] private ToolEquipmentController toolEquipment;
        [SerializeField] private ItemHoldingController itemHolding; // ✅ THÊM

        [Header("Runtime Data")]
        [SerializeField] private int selectedSlotIndex = 0;
        [SerializeField] private List<InventorySlot> hotbarSlots;
        [SerializeField] private bool isInitialized = false;

        // Properties
        public int HotbarSize => hotbarSize;
        public int SelectedSlotIndex => selectedSlotIndex;
        public bool IsInitialized => isInitialized;
        public InventorySlot SelectedSlot => GetSelectedSlot();
        public Item SelectedItem => SelectedSlot?.Item;

        // Events
        public event Action<int, int> OnSlotSelectionChanged;
        public event Action<InventorySlot> OnSelectedSlotChanged;
        public event Action<int, InventorySlot> OnHotbarSlotChanged;
        public event Action OnHotbarInitialized;

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            StartCoroutine(InitializeSequence());
        }

        private void OnDestroy()
        {
            if (inputHandler != null)
            {
                inputHandler.OnHotbarSlotSelected -= OnHotbarKeyPressed;
            }
        }

        private void OnValidate()
        {
            if (hotbarSize < 1) hotbarSize = 1;
            if (hotbarSize > 10) hotbarSize = 10;
            if (startSlotIndex < 0) startSlotIndex = 0;
        }

        // ==========================================
        // INITIALIZATION
        // ==========================================

        private IEnumerator InitializeSequence()
        {
            yield return StartCoroutine(WaitForInventoryInit());
            SetupHotbar();
            SetupInputConnection();

            isInitialized = true;
            OnHotbarInitialized?.Invoke();
        }

        private IEnumerator WaitForInventoryInit()
        {
            int waitFrames = 0;
            const int MAX_WAIT = 300;

            while (!inventoryManager.IsInitialized && waitFrames < MAX_WAIT)
            {
                waitFrames++;
                yield return null;
            }

            if (!inventoryManager.IsInitialized)
            {
                Debug.LogError("[HotbarSystem] ❌ InventoryManager failed to initialize!");
                yield break;
            }
        }

        private void ValidateReferences()
        {
            if (inventoryManager == null)
            {
                inventoryManager = GetComponent<InventoryManager>();
                if (inventoryManager == null)
                {
                    inventoryManager = FindObjectOfType<InventoryManager>();
                }
            }

            if (inventoryManager == null)
            {
                Debug.LogError("[HotbarSystem] ❌ InventoryManager not found!");
                enabled = false;
                return;
            }
        }

        private void SetupHotbar()
        {
            hotbarSlots = new List<InventorySlot>();

            for (int i = 0; i < hotbarSize; i++)
            {
                InventorySlot slot = inventoryManager.GetHotbarSlot(i);

                if (slot != null)
                {
                    hotbarSlots.Add(slot);
                    SubscribeToSlotEvents(slot, i);
                }
            }

            selectedSlotIndex = 0;
        }

        private void SetupInputConnection()
        {
            // Find references
            if (inputHandler == null)
            {
                inputHandler = FindObjectOfType<PlayerInputHandler>();
            }

            if (toolEquipment == null)
            {
                toolEquipment = FindObjectOfType<ToolEquipmentController>();
            }

            // ✅ THÊM: Find ItemHoldingController
            if (itemHolding == null)
            {
                itemHolding = FindObjectOfType<ItemHoldingController>();
            }

            // Subscribe to input
            if (inputHandler != null)
            {
                inputHandler.OnHotbarSlotSelected += OnHotbarKeyPressed;
            }
            else
            {
            }

            // ✅ THÊM: Validate ItemHoldingController
            if (itemHolding == null)
            {
            }
            else
            {
            }

            if (toolEquipment == null)
            {
            }

            if (hotbarUI == null)
                hotbarUI = FindObjectOfType<HotBarUI>();
        }

        private void SubscribeToSlotEvents(InventorySlot slot, int hotbarIndex)
        {
            slot.OnSlotChanged += (s) => OnHotbarSlotChanged?.Invoke(hotbarIndex, s);
        }

        // ==========================================
        // INPUT HANDLING
        // ==========================================

        private void OnHotbarKeyPressed(int slotIndex)
        {
            if (!isInitialized) return;
            if (slotIndex < 0 || slotIndex >= hotbarSlots.Count) return;

            if (slotIndex == selectedSlotIndex)
                return; // ✅ Không làm lại nếu chọn đúng slot đang chọn

            SelectSlot(slotIndex);
            EquipItemFromSlot(slotIndex);
            if (hotbarUI != null)
                hotbarUI.UpdateUI();
        }

        // ✅ FIXED: Equip logic với ItemHoldingController
        private void EquipItemFromSlot(int slotIndex)
        {
            InventorySlot slot = GetHotbarSlot(slotIndex);
            if (slot == null)
            {
                Debug.LogWarning("[HotbarSystem] ❌ Slot null!");
                return;
            }

            if (slot.IsEmpty)
            {
                Debug.Log("[HotbarSystem] 📭 Slot empty — unequipping all");
                toolEquipment?.UnequipTool();
                itemHolding?.UnequipItem();
                return;
            }

            Item item = slot.Item;
            if (item?.ItemData == null)
            {
                Debug.LogWarning("[HotbarSystem] ⚠️ ItemData missing!");
                return;
            }

            switch (item.ItemData.GetItemType())
            {
                case ItemType.Tool:
                    itemHolding?.UnequipItem();
                    toolEquipment?.EquipTool(item.ItemData as ToolItemData);
                    break;

                case ItemType.Seed:
                case ItemType.Consumable:
                    toolEquipment?.UnequipTool();
                    itemHolding?.EquipItem(item);
                    break;

                default:
                    toolEquipment?.UnequipTool();
                    itemHolding?.UnequipItem();
                    break;
            }
        }

        // ==========================================
        // SELECTION MANAGEMENT
        // ==========================================

        public void SelectSlot(int index)
        {
            if (index < 0 || index >= hotbarSize)
                return;

            int oldIndex = selectedSlotIndex;
            selectedSlotIndex = index;

            if (hotbarUI != null)
                hotbarUI.SelectSlot(selectedSlotIndex);

            OnSlotSelectionChanged?.Invoke(oldIndex, selectedSlotIndex);
            OnSelectedSlotChanged?.Invoke(GetSelectedSlot());

            InventorySlot slot = GetSelectedSlot();
            string itemInfo = slot?.IsEmpty == false ? slot.ItemName : "Empty";
        }

        public void SelectNextSlot()
        {
            int nextIndex = (selectedSlotIndex + 1) % hotbarSize;
            SelectSlot(nextIndex);
        }

        public void SelectPreviousSlot()
        {
            int prevIndex = (selectedSlotIndex - 1 + hotbarSize) % hotbarSize;
            SelectSlot(prevIndex);
        }

        // ==========================================
        // SLOT ACCESS
        // ==========================================

        public InventorySlot GetSelectedSlot()
        {
            if (selectedSlotIndex < 0 || selectedSlotIndex >= hotbarSlots.Count)
                return null;

            return hotbarSlots[selectedSlotIndex];
        }

        public Item GetSelectedItem()
        {
            InventorySlot slot = GetSelectedSlot();
            return slot?.Item;
        }

        public InventorySlot GetHotbarSlot(int index)
        {
            if (index < 0 || index >= hotbarSlots.Count)
                return null;

            return hotbarSlots[index];
        }

        public List<InventorySlot> GetAllHotbarSlots()
        {
            return new List<InventorySlot>(hotbarSlots);
        }

        public bool HasItemInSlot(int index)
        {
            InventorySlot slot = GetHotbarSlot(index);
            return slot != null && !slot.IsEmpty;
        }

        public bool HasSelectedItem()
        {
            return GetSelectedSlot()?.HasItem ?? false;
        }

        // ==========================================
        // ITEM OPERATIONS
        // ==========================================

        public bool UseSelectedItem()
        {
            InventorySlot selectedSlot = GetSelectedSlot();
            if (selectedSlot == null || selectedSlot.IsEmpty)
            {
                return false;
            }

            bool used = selectedSlot.UseItem();

            if (used)
            {
            }

            return used;
        }

        public Item RemoveSelectedItem(int quantity = 1)
        {
            InventorySlot selectedSlot = GetSelectedSlot();
            if (selectedSlot == null)
            {
                return null;
            }

            return selectedSlot.RemoveItem(quantity);
        }

        public bool SwapHotbarSlots(int index1, int index2)
        {
            if (!allowHotbarSwap) return false;
            if (index1 < 0 || index1 >= hotbarSize || index2 < 0 || index2 >= hotbarSize) return false;

            InventorySlot slot1 = GetHotbarSlot(index1);
            InventorySlot slot2 = GetHotbarSlot(index2);

            if (slot1 == null || slot2 == null) return false;

            return inventoryManager.SwapSlots(slot1, slot2);
        }

        // ==========================================
        // UTILITY METHODS
        // ==========================================

        public int FindHotbarSlotWithItem(string itemID)
        {
            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                if (hotbarSlots[i].ItemID == itemID)
                {
                    return i;
                }
            }
            return -1;
        }

        public bool HasItemInHotbar(string itemID)
        {
            return FindHotbarSlotWithItem(itemID) >= 0;
        }

        public int GetItemCountInHotbar(string itemID)
        {
            int count = 0;
            foreach (var slot in hotbarSlots)
            {
                if (slot.ItemID == itemID)
                {
                    count += slot.Quantity;
                }
            }
            return count;
        }

        public void ClearSelectedSlot()
        {
            InventorySlot selectedSlot = GetSelectedSlot();
            if (selectedSlot != null)
            {
                selectedSlot.Clear();
            }
        }

        // ==========================================
        // DEBUG METHODS
        // ==========================================

        [ContextMenu("Debug Hotbar")]
        private void DebugHotbar()
        {
            Debug.Log("=== HOTBAR STATE ===");
            Debug.Log($"Initialized: {isInitialized}");
            Debug.Log($"Selected Slot: {selectedSlotIndex}");

            InventorySlot selectedSlot = GetSelectedSlot();
            if (selectedSlot != null)
            {
                string itemInfo = selectedSlot.IsEmpty ? "None" : $"{selectedSlot.ItemName} x{selectedSlot.Quantity}";
                Debug.Log($"Selected Item: {itemInfo}");
            }

            Debug.Log("--- All Slots ---");
            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                InventorySlot slot = hotbarSlots[i];
                string selected = i == selectedSlotIndex ? " [SELECTED]" : "";
                string itemInfo = slot.IsEmpty ? "Empty" : $"{slot.ItemName} x{slot.Quantity}";
                Debug.Log($"Slot {i}: {itemInfo}{selected}");
            }
        }

        [ContextMenu("Select Next Slot")]
        
        [ContextMenu("Select Previous Slot")]
        private void DebugSelectPrevious()
        {
            SelectPreviousSlot();
        }

        [ContextMenu("Use Selected Item")]
        private void DebugUseSelected()
        {
            UseSelectedItem();
        }

        [ContextMenu("Test - Equip From Selected Slot")]
        private void TestEquipFromSelectedSlot()
        {
            EquipItemFromSlot(selectedSlotIndex);
        }
    }
}