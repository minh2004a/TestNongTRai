using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

[System.Serializable]
public class InventorySlot
{
    public ItemData itemData;
    public int quantity;

    // Constructor mặc định
    public InventorySlot()
    {
        itemData = null;
        quantity = 0;
    }

    // Constructor với tham số
    public InventorySlot(ItemData item, int amount)
    {
        itemData = item;
        quantity = amount;
    }

    // Kiểm tra slot có trống không
    public bool IsEmpty()
    {
        return itemData == null || quantity <= 0;
    }

    // Kiểm tra có thể thêm item vào slot này khôngg
    public bool CanAddItem(ItemData item)
    {
        return IsEmpty() || (itemData == item && quantity < itemData.maxStack);
    }

    // Thêm số lượng vào slot
    public int AddQuantity(int amount)
    {
        int newQuantity = quantity + amount;

        // Nếu vượt quá max stack size
        if (newQuantity > itemData.maxStack)
        {
            quantity = itemData.maxStack;
            // Trả về phần thừa
            return newQuantity - itemData.maxStack;
        }

        quantity = newQuantity;
        return 0; // Khong co thua
    }

    // Xóa số lượng từ slot
    public bool RemoveQuantity(int amount)
    {
        if (quantity >= amount)
        {
            quantity -= amount;

            // Nếu hết item thì clear slot
            if (quantity <= 0)
            {
                Clear();
            }
            return true;
        }
        return false;
    }

    // Xóa slot
    public void Clear()
    {
        itemData = null;
        quantity = 0;
    }

    // Clone slot (hữu ích cho drag-drop sau này)
    public InventorySlot Clone()
    {
        return new InventorySlot(itemData, quantity);
    }
}
