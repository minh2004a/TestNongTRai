using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TinyFarm.Items;
using TinyFarm.Items.UI;
using UnityEngine;

namespace TinyFarm.Items
{
    public class InventoryManager : MonoBehaviour
    {
        [Header("Inventory Settings")]
        [SerializeField] private int inventorySize = 30;
        [SerializeField] private string inventoryName = "Player Inventory";
        public static InventoryManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private ItemDatabase itemDatabase;
        public InventoryDescription descriptionPanel;

        [Header("Slots")]
        [SerializeField] private List<InventorySlot> slots;

        [Header("Runtime Data")]
        [SerializeField] private Dictionary<string, int> itemCounts;
        [SerializeField] private bool isInitialized = false;

        public int InventorySize => inventorySize;
        public string InventoryName => inventoryName;
        public bool IsInitialized => isInitialized;
        public int OccupiedSlotCount => slots.Count(s => !s.IsEmpty);
        public int EmptySlotCount => slots.Count(s => s.IsEmpty);
        public bool IsFull => EmptySlotCount == 0;

        public event Action OnInventoryChanged;
        public event Action<Item, int> OnItemAdded;
        public event Action<Item, int> OnItemRemoved;
        public event Action<InventorySlot> OnSlotChanged;
        public event Action OnInventoryFull;

        private void Awake()
        {
            Initialize();
        }

        // Khởi tạo inventory
        public void Initialize()
        {
            if (isInitialized) return;

            // Validate references
            if (itemDatabase == null)
            {
                itemDatabase = Resources.Load<ItemDatabase>("ItemDatabase");
                if (itemDatabase == null)
                {
                    Debug.LogError("[InventoryManager] ItemDatabase not found!");
                    return;
                }
            }

            // Initialize data structures
            slots = new List<InventorySlot>();
            itemCounts = new Dictionary<string, int>();

            // Create slots
            for (int i = 0; i < inventorySize; i++)
            {
                InventorySlot slot = new InventorySlot(i, SlotType.Normal);
                SubscribeToSlotEvents(slot);
                slots.Add(slot);
            }

            isInitialized = true;
            Debug.Log($"[InventoryManager] Initialized with {inventorySize} slots");
        }

        private void SubscribeToSlotEvents(InventorySlot slot)
        {
            slot.OnSlotChanged += HandleSlotChanged;
            slot.OnItemAdded += HandleItemAdded;
            slot.OnItemRemoved += HandleItemRemoved;
        }

        private void UnsubscribeFromSlotEvents(InventorySlot slot)
        {
            slot.OnSlotChanged -= HandleSlotChanged;
            slot.OnItemAdded -= HandleItemAdded;
            slot.OnItemRemoved -= HandleItemRemoved;
        }

        private void HandleSlotChanged(InventorySlot slot)
        {
            UpdateItemCounts();
            OnSlotChanged?.Invoke(slot);
            OnInventoryChanged?.Invoke();
        }

        private void HandleItemAdded(InventorySlot slot, Item item)
        {
            UpdateItemCounts();
            OnItemAdded?.Invoke(item, slot.Quantity);
        }

        private void HandleItemRemoved(InventorySlot slot, Item item)
        {
            UpdateItemCounts();
            OnItemRemoved?.Invoke(item, item.CurrentStack);
        }

        // Thêm item vào inventory
        /// Returns: True nếu add thành công (hết hoặc 1 phần)
        public bool AddItem(Item item, int quantity = -1)
        {
            if (item == null)
            {
                Debug.LogWarning("[InventoryManager] Cannot add null item!");
                return false;
            }

            int amountToAdd = quantity < 0 ? item.CurrentStack : quantity;
            int remaining = amountToAdd;

            // Try thêm vào existing stacks trước
            if (item.IsStackable)
            {
                remaining = TryAddToExistingStacks(item, remaining);
            }

            // Nếu còn dư, thêm vào empty slots
            if (remaining > 0)
            {
                remaining = TryAddToEmptySlots(item, remaining);
            }

            // Check xem có add được gì không
            bool addedAny = remaining < amountToAdd;

            if (remaining > 0)
            {
                if (!addedAny)
                {
                    Debug.LogWarning($"[InventoryManager] Inventory full! Cannot add {item.Name}");
                    OnInventoryFull?.Invoke();
                }
                else
                {
                    Debug.LogWarning($"[InventoryManager] Only added {amountToAdd - remaining}/{amountToAdd} {item.Name}");
                }
            }

            return addedAny;
        }

        /// Thêm item theo itemID
        public bool AddItem(string itemID, int quantity)
        {
            ItemData itemData = itemDatabase.GetItemByID(itemID);
            if (itemData == null)
            {
                Debug.LogError($"[InventoryManager] ItemData '{itemID}' not found!");
                return false;
            }

            Item item = CreateItemFromData(itemData, quantity);
            return AddItem(item, quantity);
        }

        private int TryAddToExistingStacks(Item item, int amount)
        {
            int remaining = amount;

            foreach (var slot in slots)
            {
                if (slot.IsEmpty) continue;
                if (!slot.Item.CanStackWith(item)) continue;
                if (slot.IsFull) continue;

                int added = slot.AddItem(item, remaining);
                remaining -= added;

                if (remaining <= 0) break;
            }

            return remaining;
        }

        private int TryAddToEmptySlots(Item item, int amount)
        {
            int remaining = amount;

            foreach (var slot in slots)
            {
                if (!slot.IsEmpty) continue;

                // Clone item với quantity phù hợp
                Item newItem = item.Clone();
                int toAdd = Mathf.Min(remaining, item.ItemData.maxStackSize);

                if (newItem.IsStackable)
                {
                    newItem.Stackable.SetStack(toAdd);
                }

                int added = slot.AddItem(newItem, toAdd);
                remaining -= added;

                if (remaining <= 0) break;
            }

            return remaining;
        }

        // Xóa item khỏi inventory theo itemID
        public bool RemoveItem(string itemID, int amount)
        {
            if (!HasItem(itemID, amount))
            {
                Debug.LogWarning($"[InventoryManager] Not enough {itemID}! Need {amount}, have {GetItemCount(itemID)}");
                return false;
            }

            int remaining = amount;

            foreach (var slot in slots)
            {
                if (slot.IsEmpty) continue;
                if (slot.ItemID != itemID) continue;

                int toRemove = Mathf.Min(remaining, slot.Quantity);
                slot.RemoveItem(toRemove);
                remaining -= toRemove;

                if (remaining <= 0) break;
            }

            return true;
        }

        /// Xóa item từ slot cụ thể
        public Item RemoveItemFromSlot(int slotIndex, int amount = 1)
        {
            InventorySlot slot = GetSlot(slotIndex);
            if (slot == null) return null;

            return slot.RemoveItem(amount);
        }

        /// Clear slot cụ thể
        public void ClearSlot(int slotIndex)
        {
            InventorySlot slot = GetSlot(slotIndex);
            if (slot != null)
            {
                slot.Clear();
            }
        }

        /// Clear tất cả inventory
        public void ClearAllSlots()
        {
            foreach (var slot in slots)
            {
                slot.Clear();
            }

            itemCounts.Clear();
            OnInventoryChanged?.Invoke();
        }

        // Kiểm tra có item này không
        public bool HasItem(string itemID, int amount = 1)
        {
            return GetItemCount(itemID) >= amount;
        }

        /// Đếm số lượng item theo itemID
        public int GetItemCount(string itemID)
        {
            if (itemCounts.ContainsKey(itemID))
            {
                return itemCounts[itemID];
            }
            return 0;
        }

        /// Lấy slot theo index
        public InventorySlot GetSlot(int index)
        {
            if (index < 0 || index >= slots.Count) return null;
            return slots[index];
        }

        /// Lấy tất cả slots
        public List<InventorySlot> GetAllSlots()
        {
            return new List<InventorySlot>(slots);
        }

        /// Lấy tất cả slots có item
        public List<InventorySlot> GetOccupiedSlots()
        {
            return slots.Where(s => !s.IsEmpty).ToList();
        }

        /// Lấy tất cả empty slots
        public List<InventorySlot> GetEmptySlots()
        {
            return slots.Where(s => s.IsEmpty).ToList();
        }

        /// Tìm slot đầu tiên có item này
        public InventorySlot FindSlotWithItem(string itemID)
        {
            return slots.FirstOrDefault(s => !s.IsEmpty && s.ItemID == itemID);
        }

        /// Tìm tất cả slots có item này
        public List<InventorySlot> FindAllSlotsWithItem(string itemID)
        {
            return slots.Where(s => !s.IsEmpty && s.ItemID == itemID).ToList();
        }

        /// Swap 2 slots
        public bool SwapSlots(int index1, int index2)
        {
            InventorySlot slot1 = GetSlot(index1);
            InventorySlot slot2 = GetSlot(index2);

            if (slot1 == null || slot2 == null) return false;

            slot1.SwapWith(slot2);
            return true;
        }

        /// Merge 2 slots (nếu cùng item type)
        public bool MergeSlots(int sourceIndex, int targetIndex)
        {
            InventorySlot source = GetSlot(sourceIndex);
            InventorySlot target = GetSlot(targetIndex);

            if (source == null || target == null) return false;

            return target.MergeWith(source);
        }

        /// Sort inventory (group same items together)
        public void SortInventory()
        {
            // Collect all items
            List<Item> allItems = new List<Item>();
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty)
                {
                    allItems.Add(slot.Item.Clone());
                }
            }

            // Clear all slots
            ClearAllSlots();

            // Sort items by name
            allItems = allItems.OrderBy(i => i.Name).ToList();

            // Re-add items
            foreach (var item in allItems)
            {
                AddItem(item);
            }

            Debug.Log("[InventoryManager] Inventory sorted!");
        }

        // Save inventory to InventoryData
        public InventoryData SaveInventory()
        {
            InventoryData data = new InventoryData(inventoryName, inventorySize);

            foreach (var slot in slots)
            {
                SlotData slotData = SlotData.FromSlot(slot);
                data.AddSlot(slotData);
            }

            data.UpdateTimestamp();

            Debug.Log($"[InventoryManager] Inventory saved: {data}");
            return data;
        }

        /// Load inventory from InventoryData
        public void LoadInventory(InventoryData data)
        {
            if (data == null)
            {
                Debug.LogError("[InventoryManager] Cannot load null InventoryData!");
                return;
            }

            if (!data.Validate())
            {
                Debug.LogError("[InventoryManager] InventoryData validation failed!");
                return;
            }

            // Clear current inventory
            ClearAllSlots();

            // Restore slots từ data
            for (int i = 0; i < data.Slots.Count && i < slots.Count; i++)
            {
                SlotData slotData = data.Slots[i];
                InventorySlot slot = slotData.ToSlot(itemDatabase);

                if (slot != null)
                {
                    // Copy data vào existing slot
                    slots[i].Clear();
                    if (!slot.IsEmpty)
                    {
                        slots[i].SetItem(slot.Item);
                    }

                    if (slotData.IsLocked)
                    {
                        slots[i].Lock();
                    }
                }
            }

            UpdateItemCounts();
            OnInventoryChanged?.Invoke();

            Debug.Log($"[InventoryManager] Inventory loaded: {data}");
        }

        private void UpdateItemCounts()
        {
            itemCounts.Clear();

            foreach (var slot in slots)
            {
                if (slot.IsEmpty) continue;

                string itemID = slot.ItemID;
                int quantity = slot.Quantity;

                if (itemCounts.ContainsKey(itemID))
                {
                    itemCounts[itemID] += quantity;
                }
                else
                {
                    itemCounts[itemID] = quantity;
                }
            }
        }

        private Item CreateItemFromData(ItemData itemData, int quantity)
        {
            Item item = null;

            switch (itemData.GetItemType())
            {
                case ItemType.Seed:
                    if (itemData is SeedItemData seedData)
                        item = new SeedItem(seedData);
                    break;

                case ItemType.Crop:
                    if (itemData is CropItemData cropData)
                        item = new CropItem(cropData);
                    break;

                case ItemType.Tool:
                    if (itemData is ToolItemData toolData)
                        item = new ToolItem(toolData);
                    break;

                case ItemType.Resources:
                    if (itemData is ResourcesItemData resourceData)
                        item = new ResourceItem(resourceData);
                    break;

                case ItemType.Equipment:
                    if (itemData is EquipmentItemData equipData)
                        item = new EquipmentItem(equipData);
                    break;

                case ItemType.CraftingMaterial:
                    if (itemData is CraftingMaterialData matData)
                        item = new CraftingMaterial(matData);
                    break;

                default:
                    // Fallback to base Item
                    item = new Item(itemData);
                    break;
            }

            if (item != null && item.IsStackable)
            {
                item.Stackable.SetStack(quantity);
            }

            return item;
        }

        [ContextMenu("Debug Inventory")]
        private void DebugInventory()
        {
            Debug.Log("=== INVENTORY STATUS ===");
            Debug.Log($"Name: {inventoryName}");
            Debug.Log($"Size: {inventorySize}");
            Debug.Log($"Occupied: {OccupiedSlotCount}/{inventorySize}");
            Debug.Log($"Empty: {EmptySlotCount}");

            Debug.Log("\n=== ITEMS ===");
            foreach (var kvp in itemCounts)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value}");
            }

            Debug.Log("\n=== SLOTS ===");
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty)
                {
                    Debug.Log($"  Slot {i}: {slots[i]}");
                }
            }
        }

        [ContextMenu("Sort Inventory")]
        private void DebugSortInventory()
        {
            SortInventory();
        }

        [ContextMenu("Clear Inventory")]
        private void DebugClearInventory()
        {
            ClearAllSlots();
        }

        private void OnDestroy()
        {
            // Cleanup
            foreach (var slot in slots)
            {
                UnsubscribeFromSlotEvents(slot);
                slot.Dispose();
            }
        }

    }

}
