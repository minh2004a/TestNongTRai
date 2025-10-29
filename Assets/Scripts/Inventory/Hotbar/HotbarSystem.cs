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
        [SerializeField] private ItemHoldingController itemHolding;

        [Header("Runtime Data")]
        [SerializeField] private int selectedSlotIndex = 0;
        [SerializeField] private List<InventorySlot> hotbarSlots;
        [SerializeField] private bool isInitialized = false;

        public int HotbarSize => hotbarSize;
        public int SelectedSlotIndex => selectedSlotIndex;
        public bool IsInitialized => isInitialized;
        public InventorySlot SelectedSlot => GetSelectedSlot();
        public Item SelectedItem => SelectedSlot?.Item;

        public event Action<int, int> OnSlotSelectionChanged;
        public event Action<InventorySlot> OnSelectedSlotChanged;
        public event Action<int, InventorySlot> OnHotbarSlotChanged;
        public event Action OnHotbarInitialized;

        private void Awake() => ValidateReferences();

        private void Start() => StartCoroutine(InitializeSequence());

        private void OnDestroy()
        {
            if (inputHandler != null)
                inputHandler.OnHotbarSlotSelected -= OnHotbarKeyPressed;
        }

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
                Debug.LogError("[HotbarSystem] ❌ InventoryManager failed to initialize!");
        }

        private void ValidateReferences()
        {
            if (inventoryManager == null)
                inventoryManager = GetComponent<InventoryManager>() ?? FindObjectOfType<InventoryManager>();

            if (inventoryManager == null)
            {
                Debug.LogError("[HotbarSystem] ❌ InventoryManager not found!");
                enabled = false;
            }
        }

        private void SetupHotbar()
        {
            hotbarSlots = new List<InventorySlot>();
            if (hotbarUI == null) hotbarUI = FindObjectOfType<HotBarUI>();

            var slotUIs = hotbarUI.GetAllSlotUIs();

            for (int i = 0; i < hotbarSize; i++)
            {
                InventorySlot slot = inventoryManager.GetHotbarSlot(i);
                if (slot != null)
                {
                    hotbarSlots.Add(slot);

                    if (i < slotUIs.Count)
                    {
                        slotUIs[i].Initialize(slot, i);

                        slotUIs[i].OnSlotClicked += (ui) =>
                        {
                            int idx = ui.SlotIndex;
                            OnHotbarSlotClicked(idx);
                        };
                    }

                    SubscribeToSlotEvents(slot, i);
                }
            }

            selectedSlotIndex = 0;

            if (hotbarSlots.Count > 0)
            {
                SelectSlot(selectedSlotIndex);
                EquipItemFromSlot(selectedSlotIndex);
                hotbarUI?.UpdateUI();
            }
        }

        private void SetupInputConnection()
        {
            if (inputHandler == null) inputHandler = FindObjectOfType<PlayerInputHandler>();
            if (toolEquipment == null) toolEquipment = FindObjectOfType<ToolEquipmentController>();
            if (itemHolding == null) itemHolding = FindObjectOfType<ItemHoldingController>();

            if (inputHandler != null)
                inputHandler.OnHotbarSlotSelected += OnHotbarKeyPressed;
        }

        private void SubscribeToSlotEvents(InventorySlot slot, int hotbarIndex)
        {
            slot.OnSlotChanged += (s) =>
            {
                OnHotbarSlotChanged?.Invoke(hotbarIndex, s);

                // Nếu slot này là slot đang chọn, equip item (kéo item mới xuống hotbar)
                if (hotbarIndex == selectedSlotIndex)
                    EquipItemFromSlot(selectedSlotIndex);
            };
        }

        public void RefreshHotbar()
        {
            // Refresh UI nếu cần
            hotbarUI?.UpdateUI();

            // Re-equip item nếu slot hiện tại còn item
            EquipItemFromSlot(selectedSlotIndex);
        }
        
        

        // ==========================================
        // INPUT HANDLING
        // ==========================================

        private void OnHotbarKeyPressed(int slotIndex)
        {
            if (!isInitialized) return;
            if (slotIndex < 0 || slotIndex >= hotbarSlots.Count) return;

            if (slotIndex != selectedSlotIndex)
            {
                SelectSlot(slotIndex);
                EquipItemFromSlot(slotIndex);
                hotbarUI?.UpdateUI();
            }
        }

        private void OnHotbarSlotClicked(int slotIndex)
        {
            if (!isInitialized) return;
            if (slotIndex < 0 || slotIndex >= hotbarSlots.Count) return;

            // Click slot → select + equip nhưng không trigger UseItem
            if (slotIndex != selectedSlotIndex)
            {
                SelectSlot(slotIndex);
                EquipItemFromSlot(slotIndex);
                hotbarUI?.UpdateUI();
            }
        }

        // ==========================================
        // EQUIP LOGIC
        // ==========================================

        private void EquipItemFromSlot(int slotIndex)
        {
            InventorySlot slot = GetHotbarSlot(slotIndex);
            if (slot == null)
            {
                toolEquipment?.UnequipTool();
                itemHolding?.UnequipItem();
                return;
            }

            if (slot.IsEmpty)
            {
                toolEquipment?.UnequipTool();
                itemHolding?.UnequipItem();
                return;
            }

            Item item = slot.Item;
            if (item?.ItemData == null) return;

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
            if (index < 0 || index >= hotbarSize) return;

            int oldIndex = selectedSlotIndex;
            selectedSlotIndex = index;

            hotbarUI?.SelectSlot(selectedSlotIndex);

            OnSlotSelectionChanged?.Invoke(oldIndex, selectedSlotIndex);
            OnSelectedSlotChanged?.Invoke(GetSelectedSlot());
        }

        public void SelectNextSlot() => SelectSlot((selectedSlotIndex + 1) % hotbarSize);
        public void SelectPreviousSlot() => SelectSlot((selectedSlotIndex - 1 + hotbarSize) % hotbarSize);

        // ==========================================
        // SLOT ACCESS
        // ==========================================

        public InventorySlot GetSelectedSlot() => selectedSlotIndex >= 0 && selectedSlotIndex < hotbarSlots.Count ? hotbarSlots[selectedSlotIndex] : null;
        public Item GetSelectedItem() => GetSelectedSlot()?.Item;
        public InventorySlot GetHotbarSlot(int index) => (index >= 0 && index < hotbarSlots.Count) ? hotbarSlots[index] : null;
        public List<InventorySlot> GetAllHotbarSlots() => new List<InventorySlot>(hotbarSlots);
        public bool HasItemInSlot(int index) => GetHotbarSlot(index) != null && !GetHotbarSlot(index).IsEmpty;
        public bool HasSelectedItem() => GetSelectedSlot()?.HasItem ?? false;

        // ==========================================
        // ITEM OPERATIONS
        // ==========================================

        public bool UseSelectedItem() => GetSelectedSlot()?.UseItem() ?? false;

        public Item RemoveSelectedItem(int quantity = 1) => GetSelectedSlot()?.RemoveItem(quantity);

        public bool SwapHotbarSlots(int index1, int index2)
        {
            if (!allowHotbarSwap) return false;
            InventorySlot slot1 = GetHotbarSlot(index1);
            InventorySlot slot2 = GetHotbarSlot(index2);
            if (slot1 == null || slot2 == null) return false;
            return inventoryManager.SwapSlots(slot1, slot2);
        }

        // ==========================================
        // UTILITY
        // ==========================================

        public int FindHotbarSlotWithItem(string itemID)
        {
            for (int i = 0; i < hotbarSlots.Count; i++)
                if (hotbarSlots[i].ItemID == itemID) return i;
            return -1;
        }

        public bool HasItemInHotbar(string itemID) => FindHotbarSlotWithItem(itemID) >= 0;

        public int GetItemCountInHotbar(string itemID)
        {
            int count = 0;
            foreach (var slot in hotbarSlots)
                if (slot.ItemID == itemID) count += slot.Quantity;
            return count;
        }

        public void ClearSelectedSlot() => GetSelectedSlot()?.Clear();
    }
}