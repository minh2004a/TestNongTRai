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
        [SerializeField] private int startSlotIndex = 0; // Slot bắt đầu trong inventory
        [SerializeField] private bool allowHotbarSwap = true;

        [Header("References")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private PlayerInputHandler inputHandler;           
        [SerializeField] private ToolEquipmentController toolEquipment;

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
        public event Action<int, int> OnSlotSelectionChanged; // oldIndex, newIndex
        public event Action<InventorySlot> OnSelectedSlotChanged;
        public event Action<int, InventorySlot> OnHotbarSlotChanged; // slotIndex, slot
        public event Action OnHotbarInitialized;

        private void Awake()
        {
            ValidateReferences();
        }

        private void Start()
        {
            // Initialize in correct order
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
        // INITIALIZATION SEQUENCE (FIXED)
        // ==========================================

        private IEnumerator InitializeSequence()
        {
            // Step 1: Wait for InventoryManager
            yield return StartCoroutine(WaitForInventoryInit());

            // Step 2: Setup hotbar slots
            SetupHotbar();

            // Step 3: Setup input connections
            SetupInputConnection();

            // Step 4: Mark as initialized
            isInitialized = true;
            OnHotbarInitialized?.Invoke();

            DebugHotbar(); // Auto log initial state
        }

        private IEnumerator WaitForInventoryInit()
        {
            int waitFrames = 0;
            const int MAX_WAIT = 300; // 5 seconds at 60fps

            while (!inventoryManager.IsInitialized && waitFrames < MAX_WAIT)
            {
                waitFrames++;
                yield return null;
            }

            if (!inventoryManager.IsInitialized)
            {
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
                enabled = false;
                return;
            }
        }

        private void SetupHotbar()
        {
            hotbarSlots = new List<InventorySlot>();

            for (int i = 0; i < hotbarSize; i++)
            {
                int slotIndex = startSlotIndex + i;
                InventorySlot slot = inventoryManager.GetSlot(slotIndex);

                if (slot != null)
                {
                    hotbarSlots.Add(slot);
                    SubscribeToSlotEvents(slot, i);

                    // Debug each slot
                    string itemInfo = slot.IsEmpty ? "Empty" : $"{slot.ItemName} x{slot.Quantity}";
                }
                else
                {
                    Debug.LogError($"[HotbarSystem] ❌ Cannot get slot {slotIndex} from inventory!");
                }
            }

            selectedSlotIndex = 0;
        }

        private void SetupInputConnection()
        {
            // Find references if not assigned
            if (inputHandler == null)
            {
                inputHandler = FindObjectOfType<PlayerInputHandler>();
            }

            if (toolEquipment == null)
            {
                toolEquipment = FindObjectOfType<ToolEquipmentController>();
            }

            // Subscribe to input events
            if (inputHandler != null)
            {
                inputHandler.OnHotbarSlotSelected += OnHotbarKeyPressed;
            }
            else
            {
                Debug.LogError("[HotbarSystem] ❌ PlayerInputHandler not found!");
            }

            if (toolEquipment == null)
            {
                Debug.LogWarning("[HotbarSystem] ⚠️ ToolEquipmentController not found!");
            }
        }

        private void SubscribeToSlotEvents(InventorySlot slot, int hotbarIndex)
        {
            slot.OnSlotChanged += (s) => OnHotbarSlotChanged?.Invoke(hotbarIndex, s);
        }

        // ==========================================
        // INPUT HANDLING (IMPROVED DEBUG)
        // ==========================================

        private void OnHotbarKeyPressed(int slotIndex)
        {

            if (!isInitialized)
            {
                return;
            }

            if (slotIndex < 0 || slotIndex >= hotbarSlots.Count)
            {
                return;
            }

            // Select slot
            SelectSlot(slotIndex);

            // Get slot info
            InventorySlot slot = GetHotbarSlot(slotIndex);
            if (slot != null)
            {
                if (slot.IsEmpty)
                {
                    Debug.Log($"[HotbarSystem] 📭 Slot {slotIndex} is empty");
                }
                else
                {
                    Debug.Log($"[HotbarSystem] 📦 Slot {slotIndex}: {slot.ItemName} x{slot.Quantity}");
                }
            }

            // Try equip tool
            EquipToolFromSlot(slotIndex);
        }

        private void EquipToolFromSlot(int slotIndex)
        {
            if (toolEquipment == null)
            {
                return;
            }

            InventorySlot slot = GetHotbarSlot(slotIndex);

            if (slot == null)
            {
                return;
            }

            if (slot.IsEmpty)
            {
                toolEquipment.UnequipTool();
                return;
            }

            Item item = slot.Item;

            // Check if item is a tool
            if (item?.ItemData == null)
            {
                return;
            }

            ItemType itemType = item.ItemData.GetItemType();

            if (itemType == ItemType.Tool)
            {
                ToolItemData toolData = item.ItemData as ToolItemData;

                if (toolData != null)
                {
                    bool success = toolEquipment.EquipTool(toolData);

                    if (success)
                    {
                    }
                    else
                    {
                    }
                }
                else
                {
                }
            }
            else
            {
                toolEquipment.UnequipTool();
            }
        }

        // ==========================================
        // SELECTION MANAGEMENT
        // ==========================================

        public void SelectSlot(int index)
        {
            if (index < 0 || index >= hotbarSize)
            {
                return;
            }

            int oldIndex = selectedSlotIndex;
            selectedSlotIndex = index;

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
                Debug.LogWarning("[HotbarSystem] No item selected to use!");
                return false;
            }

            bool used = selectedSlot.UseItem();

            if (used)
            {
                Debug.Log($"[HotbarSystem] Used {selectedSlot.ItemName}");
            }

            return used;
        }

        public Item RemoveSelectedItem(int quantity = 1)
        {
            InventorySlot selectedSlot = GetSelectedSlot();
            if (selectedSlot == null)
            {
                Debug.LogWarning("[HotbarSystem] No slot selected!");
                return null;
            }

            return selectedSlot.RemoveItem(quantity);
        }

        public bool SwapHotbarSlots(int index1, int index2)
        {
            if (!allowHotbarSwap)
            {
                Debug.LogWarning("[HotbarSystem] Hotbar swap is disabled!");
                return false;
            }

            if (index1 < 0 || index1 >= hotbarSize || index2 < 0 || index2 >= hotbarSize)
            {
                Debug.LogWarning("[HotbarSystem] Invalid hotbar indices!");
                return false;
            }

            int invIndex1 = startSlotIndex + index1;
            int invIndex2 = startSlotIndex + index2;

            return inventoryManager.SwapSlots(invIndex1, invIndex2);
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
            InventorySlot selectedSlot = GetSelectedSlot();
            if (selectedSlot != null)
            {
                string itemInfo = selectedSlot.IsEmpty ? "None" : $"{selectedSlot.ItemName} x{selectedSlot.Quantity}";
            }

            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                InventorySlot slot = hotbarSlots[i];
                string selected = i == selectedSlotIndex ? " [SELECTED]" : "";
                string itemInfo = slot.IsEmpty ? "Empty" : $"{slot.ItemName} x{slot.Quantity}";
            }
        }

        [ContextMenu("Select Next Slot")]
        private void DebugSelectNext()
        {
            SelectNextSlot();
        }

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

        [ContextMenu("Test - Equip Tool From Selected Slot")]
        private void TestEquipToolFromSelectedSlot()
        {
            EquipToolFromSlot(selectedSlotIndex);
        }

    }
}