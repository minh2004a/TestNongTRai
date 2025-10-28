using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    [System.Serializable]
    public class InventorySlot
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private ItemStack itemStack;
        [SerializeField] private bool isLocked;
        [SerializeField] private SlotType slotType;

        public int SlotIndex => slotIndex;
        public ItemStack ItemStack => itemStack;
        public bool IsLocked => isLocked;
        public SlotType SlotType => slotType;

        // Computed properties
        public bool IsEmpty => itemStack?.IsEmpty ?? true;
        public bool HasItem => !IsEmpty;
        public Item Item => itemStack?.Item;
        public ItemData ItemData => itemStack?.Item?.ItemData;
        public int Quantity => itemStack?.Quantity ?? 0;
        public bool IsFull => itemStack?.IsFull ?? false;

        // Quick accessors
        public string ItemName => itemStack?.ItemName ?? "Empty";
        public Sprite ItemIcon => itemStack?.ItemIcon;
        public string ItemID => itemStack?.ItemID;

        public event Action<InventorySlot> OnSlotChanged;
        public event Action<InventorySlot, Item> OnItemAdded;
        public event Action<InventorySlot, Item> OnItemRemoved;
        public event Action<InventorySlot> OnSlotCleared;
        public event Action<bool> OnSlotLockChanged;

        public InventorySlot(int index, SlotType type = SlotType.Normal)
        {
            slotIndex = index;
            slotType = type;
            itemStack = new ItemStack();
            isLocked = false;

            SubscribeToStackEvents();
        }

        private void SubscribeToStackEvents()
        {
            if (itemStack != null)
            {
                itemStack.OnStackChanged += HandleStackChanged;
                itemStack.OnItemChanged += HandleItemChanged;
                itemStack.OnStackEmpty += HandleStackEmpty;
            }
        }

        private void UnsubscribeFromStackEvents()
        {
            if (itemStack != null)
            {
                itemStack.OnStackChanged -= HandleStackChanged;
                itemStack.OnItemChanged -= HandleItemChanged;
                itemStack.OnStackEmpty -= HandleStackEmpty;
            }
        }

        private void HandleStackChanged(ItemStack stack)
        {
            OnSlotChanged?.Invoke(this);
        }

        private void HandleItemChanged(ItemStack stack)
        {
            OnSlotChanged?.Invoke(this);
        }

        private void HandleStackEmpty(ItemStack stack)
        {
            OnSlotCleared?.Invoke(this);
            OnSlotChanged?.Invoke(this);
        }

        // Thêm item vào slot
        /// Returns: Số lượng đã thêm thành công
        public int AddItem(Item item, int quantity = -1)
        {
            if (item == null)
            {
                Debug.LogWarning("[InventorySlot] Cannot add null item!");
                return 0;
            }

            if (isLocked)
            {
                Debug.LogWarning($"[InventorySlot] Slot {slotIndex} is locked!");
                return 0;
            }

            // Validate slot type
            if (!CanAcceptItem(item))
            {
                Debug.LogWarning($"[InventorySlot] Slot {slotIndex} cannot accept {item.Name}!");
                return 0;
            }

            int amountToAdd = quantity < 0 ? item.CurrentStack : quantity;
            int added = itemStack.AddItem(item.Clone(), amountToAdd);

            if (added > 0)
            {
                OnItemAdded?.Invoke(this, item);
                OnSlotChanged?.Invoke(this);
            }

            return added;
        }

        // Xóa item khỏi slot
        /// Returns: Item đã xóa
        public Item RemoveItem(int quantity = 1)
        {
            if (IsEmpty)
            {
                Debug.LogWarning($"[InventorySlot] Slot {slotIndex} is empty!");
                return null;
            }

            if (isLocked)
            {
                Debug.LogWarning($"[InventorySlot] Slot {slotIndex} is locked!");
                return null;
            }

            Item removedItem = itemStack.RemoveItem(quantity);

            if (removedItem != null)
            {
                OnItemRemoved?.Invoke(this, removedItem);
                OnSlotChanged?.Invoke(this);
            }

            return removedItem;
        }

        /// Xóa tất cả items khỏi slot
        public Item RemoveAll()
        {
            return RemoveItem(Quantity);
        }

        /// Clear slot (xóa item không return)
        public void Clear()
        {
            if (IsEmpty) return;

            if (isLocked)
            {
                Debug.LogWarning($"[InventorySlot] Slot {slotIndex} is locked!");
                return;
            }

            itemStack.Clear();
            OnSlotCleared?.Invoke(this);
            OnSlotChanged?.Invoke(this);
        }

        /// Set item trực tiếp vào slot
        public void SetItem(Item item)
        {
            if (isLocked)
            {
                Debug.LogWarning($"[InventorySlot] Slot {slotIndex} is locked!");
                return;
            }

            // ✅ Nếu null → clear slot an toàn
            if (item == null)
            {
                itemStack.Clear();
                OnSlotCleared?.Invoke(this);
                OnSlotChanged?.Invoke(this);
                return;
            }

            // ✅ Clone item hợp lệ
            itemStack.SetItem(item.Clone());
            OnSlotChanged?.Invoke(this);
        }

        // Swap với slot khác
        public void SwapWith(InventorySlot otherSlot)
        {
            if (otherSlot == null || isLocked || otherSlot.isLocked)
                return;

            // ✅ Backup hai item
            Item tempThis = this.Item?.Clone();
            Item tempOther = otherSlot.Item?.Clone();

            // ✅ Gán trực tiếp, không gọi lồng SetItem (tránh gọi event hai lần)
            this.itemStack.SetItem(tempOther);
            otherSlot.itemStack.SetItem(tempThis);

            // ✅ Bắn event để UI cập nhật
            OnSlotChanged?.Invoke(this);
            otherSlot.OnSlotChanged?.Invoke(otherSlot);
        }

        /// Merge với slot khác
        public bool MergeWith(InventorySlot otherSlot)
        {
            if (otherSlot == null) return false;
            if (isLocked || otherSlot.isLocked) return false;

            bool merged = itemStack.MergeWith(otherSlot.itemStack);

            if (merged)
            {
                OnSlotChanged?.Invoke(this);
                otherSlot.OnSlotChanged?.Invoke(otherSlot);
            }

            return merged;
        }

        /// Split stack trong slot
        public ItemStack Split(int quantity)
        {
            if (IsEmpty || isLocked) return null;

            ItemStack splitStack = itemStack.Split(quantity);

            if (splitStack != null)
            {
                OnSlotChanged?.Invoke(this);
            }

            return splitStack;
        }

        /// Sử dụng item trong slot
        public bool UseItem()
        {
            if (IsEmpty || isLocked) return false;

            bool used = itemStack.UseItem();

            if (used)
            {
                OnSlotChanged?.Invoke(this);
            }

            return used;
        }

        // Kiểm tra slot có thể chấp nhận item này không
        public bool CanAcceptItem(Item item)
        {
            if (item == null) return false;
            if (isLocked) return false;

            // Check slot type compatibility
            switch (slotType)
            {
                case SlotType.Normal:
                    return true; // Accept all items

                case SlotType.ToolOnly:
                    return item.ItemData.itemType == ItemType.Tool;

                case SlotType.SeedOnly:
                    return item.ItemData.itemType == ItemType.Seed;

                case SlotType.CropOnly:
                    return item.ItemData.itemType == ItemType.Crop;

                case SlotType.EquipmentOnly:
                    return item.ItemData.itemType == ItemType.Equipment;

                default:
                    return true;
            }
        }

        /// Kiểm tra có thể swap với slot khác không
        public bool CanSwapWith(InventorySlot otherSlot)
        {
            if (otherSlot == null) return false;
            if (isLocked || otherSlot.isLocked) return false;

            return itemStack.CanSwapWith(otherSlot.itemStack);
        }

        /// Kiểm tra có thể merge với slot khác không
        public bool CanMergeWith(InventorySlot otherSlot)
        {
            if (otherSlot == null) return false;
            if (isLocked || otherSlot.isLocked) return false;
            if (IsEmpty || otherSlot.IsEmpty) return false;

            return Item.CanStackWith(otherSlot.Item) && !IsFull;
        }

        // Khóa slot (không thể thao tác)
        public void Lock()
        {
            if (isLocked) return;

            isLocked = true;
            OnSlotLockChanged?.Invoke(true);
            Debug.Log($"[InventorySlot] Slot {slotIndex} locked");
        }

        /// Mở khóa slot
        public void Unlock()
        {
            if (!isLocked) return;

            isLocked = false;
            OnSlotLockChanged?.Invoke(false);
            Debug.Log($"[InventorySlot] Slot {slotIndex} unlocked");
        }


        public override string ToString()
        {
            string lockStatus = isLocked ? "[LOCKED]" : "";
            string typeStatus = slotType != SlotType.Normal ? $"[{slotType}]" : "";
            return $"[Slot {slotIndex}] {lockStatus}{typeStatus} {itemStack}";
        }

        /// Clone slot
        public InventorySlot Clone()
        {
            InventorySlot clone = new InventorySlot(slotIndex, slotType);
            clone.isLocked = isLocked;

            if (!IsEmpty)
            {
                clone.itemStack = itemStack.Clone();
            }

            return clone;
        }

        /// Cleanup khi destroy
        public void Dispose()
        {
            UnsubscribeFromStackEvents();
        }
    }
}

