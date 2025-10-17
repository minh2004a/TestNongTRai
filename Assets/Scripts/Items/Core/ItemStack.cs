using System;
using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

public class ItemStack
{
    [SerializeField] private Item item;

    // Events
    public event Action<ItemStack> OnStackChanged;
    public event Action<ItemStack> OnItemChanged;
    public event Action<ItemStack> OnStackEmpty;

    // Properties
    public Item Item => item;
    public bool IsEmpty => item == null || (item.IsStackable && item.CurrentStack <= 0);
    public bool HasItem => !IsEmpty;
    public int Quantity => item?.CurrentStack ?? 0;
    public int MaxStackSize => item?.Stackable?.MaxStackSize ?? 0;
    public bool IsFull => item?.Stackable?.IsFull ?? false;
    public int RemainingSpace => item?.Stackable?.RemainingSpace ?? 0;

    // Quick accessors
    public string ItemName => item?.Name ?? "Empty";
    public Sprite ItemIcon => item?.Icon;

    public string ItemID => item?.ID;
    public MaterialTier MaterialTier => item?.materialTier ?? MaterialTier.Common;

    // ===== CONSTRUCTORS =====

    public ItemStack()
    {
        this.item = null;
    }

    public ItemStack(Item item)
    {
        SetItem(item);
    }

    /// Set item mới cho stack
    public void SetItem(Item newItem)
    {
        // Unsubscribe old item
        if (item != null && item.Stackable != null)
        {
            item.Stackable.OnStackChanged -= OnItemStackChanged;
        }

        item = newItem;

        // Subscribe new item
        if (item != null && item.Stackable != null)
        {
            item.Stackable.OnStackChanged += OnItemStackChanged;
        }

        OnItemChanged?.Invoke(this);

        if (IsEmpty)
        {
            OnStackEmpty?.Invoke(this);
        }
    }

    // Clear stack (remove item)
    public void Clear()
    {
        SetItem(null);
    }

    // Thêm item vào stack
    public int AddItem(Item itemToAdd, int quantity = -1)
    {
        if (itemToAdd == null) return 0;

        // Nếu stack rỗng, set item mới
        if (IsEmpty)
        {
            SetItem(itemToAdd);
            return itemToAdd.CurrentStack;
        }

        // Kiểm tra có thể stack không
        if (!item.CanStackWith(itemToAdd))
        {
            return 0; // Không thể add
        }

        // Determine quantity to add
        int toAdd = quantity < 0 ? itemToAdd.CurrentStack : quantity;

        // Merge stacks
        int overflow = item.Stackable.AddToStack(toAdd);
        return toAdd - overflow; // Return số lượng đã add thành công
    }

    // Lấy item từ stack
    public Item RemoveItem(int quantity = 1)
    {
        if (IsEmpty) return null;

        // Nếu lấy hết hoặc item không stackable
        if (quantity >= Quantity || !item.IsStackable)
        {
            Item removedItem = item;
            Clear();
            return removedItem;
        }

        // Split stack
        StackableProperty splitStack = item.Stackable.Split(quantity);
        if (splitStack != null)
        {
            // Tạo item mới với stack đã split
            Item newItem = item.Clone();
            newItem.Stackable.SetStack(splitStack.CurrentStack);
            return newItem;
        }

        return null;
    }

    // Lấy ALL items từ stack
    public Item RemoveAll()
    {
        return RemoveItem(Quantity);
    }

    // Split stack thành 2 phần
    public ItemStack Split(int quantity)
    {
        Item splitItem = RemoveItem(quantity);
        return splitItem != null ? new ItemStack(splitItem) : null;
    }

    public bool CanSwapWith(ItemStack other)
    {
        if (other == null) return false;

        // Empty stacks có thể swap
        if (IsEmpty || other.IsEmpty) return true;

        // Cùng loại item có thể swap (để merge)
        if (item.IsSameType(other.Item)) return true;

        // Khác loại cũng có thể swap (normal swap)
        return true;
    }

    // Merge stack khác vào stack này
    public bool MergeWith(ItemStack other)
    {
        if (other == null || other.IsEmpty) return false;

        if (IsEmpty)
        {
            SetItem(other.RemoveAll());
            return true;
        }

        if (!item.CanStackWith(other.Item)) return false;

        int added = AddItem(other.Item);

        // Remove số lượng đã merge từ other stack
        if (added > 0)
        {
            other.Item.Stackable.RemoveFromStack(added);

            if (other.IsEmpty)
            {
                other.Clear();
            }

            return true;
        }

        return false;
    }

    // Swap items với stack khác
    public void SwapWith(ItemStack other)
    {
        if (other == null) return;

        Item temp = this.item;
        this.SetItem(other.item);
        other.SetItem(temp);
    }

    // ===== ITEM OPERATIONS =====

    // Sử dụng item trong stack
    public bool UseItem()
    {
        if (IsEmpty) return false;

        bool success = item.Use();

        if (success && IsEmpty)
        {
            Clear();
        }

        return success;
    }

    // Kiểm tra có thể chấp nhận item không
    public bool CanAccept(Item itemToCheck)
    {
        if (itemToCheck == null) return false;
        if (IsEmpty) return true;
        return item.CanStackWith(itemToCheck) && !IsFull;
    }

    // ===== EVENT HANDLERS =====

    private void OnItemStackChanged(int newStack)
    {
        OnStackChanged?.Invoke(this);

        if (IsEmpty)
        {
            Clear();
        }
    }

    // ===== UTILITY =====

    public override string ToString()
    {
        return IsEmpty ? "Empty Stack" : $"{item} ({Quantity}/{MaxStackSize})";
    }

    // Clone stack
    public ItemStack Clone()
    {
        if (IsEmpty) return new ItemStack();
        return new ItemStack(item.Clone());
    }
}
