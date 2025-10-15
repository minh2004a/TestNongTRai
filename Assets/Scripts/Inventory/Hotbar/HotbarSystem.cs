using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items.UI 
{
    public class HotbarSystem : MonoBehaviour
    {
        [Header("Hotbar Settings")]
        [SerializeField] private int hotbarSize = 12;
        [SerializeField] private int startSlotIndex = 0; // Slot bắt đầu trong inventory
        [SerializeField] private bool allowHotbarSwap = true;

        [Header("References")]
        [SerializeField] private InventoryManager inventoryManager;

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
            Initialize();
        }

        private void Update()
        {
            // Hotkey selection (1-9, 0 for slot 10)
            HandleHotkeyInput();

            // Mouse scroll wheel selection
            HandleScrollInput();
        }

        // Khởi tạo hotbar system
        /// </summary>
        public void Initialize()
        {
            if (isInitialized) return;

            // Validate references
            if (inventoryManager == null)
            {
                inventoryManager = GetComponent<InventoryManager>();
                if (inventoryManager == null)
                {
                    inventoryManager = FindObjectOfType<InventoryManager>();
                }

                if (inventoryManager == null)
                {
                    Debug.LogError("[HotbarSystem] InventoryManager not found!");
                    return;
                }
            }

            // Wait for inventory to initialize
            if (!inventoryManager.IsInitialized)
            {
                Debug.LogWarning("[HotbarSystem] Waiting for InventoryManager to initialize...");
                StartCoroutine(WaitForInventoryInit());
                return;
            }

            SetupHotbar();
        }

        private System.Collections.IEnumerator WaitForInventoryInit()
        {
            while (!inventoryManager.IsInitialized)
            {
                yield return null;
            }
            SetupHotbar();
        }

        private void SetupHotbar()
        {
            hotbarSlots = new List<InventorySlot>();

            // Lấy reference đến các slots từ inventory
            for (int i = 0; i < hotbarSize; i++)
            {
                int slotIndex = startSlotIndex + i;
                InventorySlot slot = inventoryManager.GetSlot(slotIndex);

                if (slot != null)
                {
                    hotbarSlots.Add(slot);
                    SubscribeToSlotEvents(slot, i);
                }
                else
                {
                    Debug.LogWarning($"[HotbarSystem] Cannot get slot {slotIndex} from inventory!");
                }
            }

            // Set slot đầu tiên là selected
            selectedSlotIndex = 0;

            isInitialized = true;
            OnHotbarInitialized?.Invoke();
            Debug.Log($"[HotbarSystem] Initialized with {hotbarSlots.Count} slots");
        }

        private void SubscribeToSlotEvents(InventorySlot slot, int hotbarIndex)
        {
            slot.OnSlotChanged += (s) => OnHotbarSlotChanged?.Invoke(hotbarIndex, s);
        }

        #region Selection Management

        // Chọn slot theo index (0-based)
        /// </summary>
        public void SelectSlot(int index)
        {
            if (index < 0 || index >= hotbarSize)
            {
                Debug.LogWarning($"[HotbarSystem] Invalid slot index: {index}");
                return;
            }

            int oldIndex = selectedSlotIndex;
            selectedSlotIndex = index;

            OnSlotSelectionChanged?.Invoke(oldIndex, selectedSlotIndex);
            OnSelectedSlotChanged?.Invoke(GetSelectedSlot());

            Debug.Log($"[HotbarSystem] Selected slot {selectedSlotIndex}: {GetSelectedSlot()?.ItemName ?? "Empty"}");
        }

        /// <summary>
        /// Chọn slot kế tiếp
        /// </summary>
        public void SelectNextSlot()
        {
            int nextIndex = (selectedSlotIndex + 1) % hotbarSize;
            SelectSlot(nextIndex);
        }

        /// <summary>
        /// Chọn slot trước đó
        /// </summary>
        public void SelectPreviousSlot()
        {
            int prevIndex = (selectedSlotIndex - 1 + hotbarSize) % hotbarSize;
            SelectSlot(prevIndex);
        }

        /// <summary>
        /// Lấy slot đang được chọn
        /// </summary>
        public InventorySlot GetSelectedSlot()
        {
            if (selectedSlotIndex < 0 || selectedSlotIndex >= hotbarSlots.Count)
                return null;

            return hotbarSlots[selectedSlotIndex];
        }

        /// <summary>
        /// Lấy item đang được chọn
        /// </summary>
        public Item GetSelectedItem()
        {
            InventorySlot slot = GetSelectedSlot();
            return slot?.Item;
        }

#endregion

        #region Slot Access

        /// <summary>
        /// Lấy hotbar slot theo index
        /// </summary>
        public InventorySlot GetHotbarSlot(int index)
        {
            if (index < 0 || index >= hotbarSlots.Count)
                return null;

            return hotbarSlots[index];
        }

        /// <summary>
        /// Lấy tất cả hotbar slots
        /// </summary>
        public List<InventorySlot> GetAllHotbarSlots()
        {
            return new List<InventorySlot>(hotbarSlots);
        }

        /// <summary>
        /// Kiểm tra slot có item không
        /// </summary>
        public bool HasItemInSlot(int index)
        {
            InventorySlot slot = GetHotbarSlot(index);
            return slot != null && !slot.IsEmpty;
        }

        /// <summary>
        /// Kiểm tra selected slot có item không
        /// </summary>
        public bool HasSelectedItem()
        {
            return GetSelectedSlot()?.HasItem ?? false;
        }

        #endregion

        #region Item Operations

        // Sử dụng item trong selected slot
        /// </summary>
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

        /// <summary>
        /// Xóa item khỏi selected slot
        /// </summary>
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

        /// <summary>
        /// Swap 2 hotbar slots
        /// </summary>
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

            // Get actual inventory slot indices
            int invIndex1 = startSlotIndex + index1;
            int invIndex2 = startSlotIndex + index2;

            return inventoryManager.SwapSlots(invIndex1, invIndex2);
        }

        #endregion

        #region Input Handling
        // Xử lý hotkey input (1-9, 0)
        /// </summary>
        private void HandleHotkeyInput()
        {
            // Number keys 1-9
            for (int i = 1; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i))
                {
                    int slotIndex = i - 1; // 0-based index
                    if (slotIndex < hotbarSize)
                    {
                        SelectSlot(slotIndex);
                    }
                    return;
                }
            }

            // Key 0 for slot 10 (if exists)
            if (Input.GetKeyDown(KeyCode.Alpha0))
            {
                if (hotbarSize >= 10)
                {
                    SelectSlot(9);
                }
            }
        }

        /// <summary>
        /// Xử lý mouse scroll input
        /// </summary>
        private void HandleScrollInput()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");

            if (scroll > 0f)
            {
                // Scroll up - previous slot
                SelectPreviousSlot();
            }
            else if (scroll < 0f)
            {
                // Scroll down - next slot
                SelectNextSlot();
            }
        }

        #endregion

        #region Utility Methods

        // Tìm slot trong hotbar có item ID này
        /// </summary>
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

        /// <summary>
        /// Kiểm tra hotbar có item này không
        /// </summary>
        public bool HasItemInHotbar(string itemID)
        {
            return FindHotbarSlotWithItem(itemID) >= 0;
        }

        /// <summary>
        /// Đếm số lượng item trong hotbar
        /// </summary>
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

        /// <summary>
        /// Clear selected slot
        /// </summary>
        public void ClearSelectedSlot()
        {
            InventorySlot selectedSlot = GetSelectedSlot();
            if (selectedSlot != null)
            {
                selectedSlot.Clear();
            }
        }

        #endregion

        #region Debug Methods

        [ContextMenu("Debug Hotbar")]
        private void DebugHotbar()
        {
            Debug.Log("=== HOTBAR STATUS ===");
            Debug.Log($"Size: {hotbarSize}");
            Debug.Log($"Selected: Slot {selectedSlotIndex}");
            Debug.Log($"Selected Item: {GetSelectedItem()?.Name ?? "None"}");

            Debug.Log("\n=== HOTBAR SLOTS ===");
            for (int i = 0; i < hotbarSlots.Count; i++)
            {
                string selected = i == selectedSlotIndex ? " [SELECTED]" : "";
                Debug.Log($"  Hotbar[{i}]: {hotbarSlots[i]}{selected}");
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

        #endregion
        private void OnDestroy()
        {
            // Cleanup - không cần unsubscribe vì slots thuộc về inventory manager
        }

        private void OnValidate()
        {
            if (hotbarSize < 1) hotbarSize = 1;
            if (hotbarSize > 10) hotbarSize = 10;
            if (startSlotIndex < 0) startSlotIndex = 0;
        }
    }
}