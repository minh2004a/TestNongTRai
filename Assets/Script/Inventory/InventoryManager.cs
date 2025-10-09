using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

public class InventoryManager : GameMonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Inventory Settings")]
    [SerializeField] private int inventorySize = 20; // Số slot trong inventoryy

    private List<InventorySlot> slots;

    // Event để thông báo khi inventory thay đổi (dùng cho UI sau này)
    public delegate void OnInventoryChanged();
    public event OnInventoryChanged onInventoryChanged;

    protected override void Awake()
    {
        base.Awake();
        // Setup Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            this.InitializeInventory();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Khởi tạo inventory với các slot trống
    private void InitializeInventory()
    {
        slots = new List<InventorySlot>();
        for (int i =0; i < inventorySize; i++)
        {
            slots.Add(new InventorySlot());
        }
        Debug.Log($"Inventory {inventorySize} slots");
    }

    // Thêm item vào inventory
    public bool AddItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null || quantity <= 0)
        {
            Debug.LogWarning("Cannot add invalid item or quantity");
            return false;
        }

        int remainingQuantity = quantity;

        // Bước 1: Tìm các slot đã có item này và chưa đầy
        for (int i = 0; i < slots.Count && remainingQuantity > 0; i++)
        {
            if (!slots[i].IsEmpty() && slots[i].itemData == itemData)
            {
                remainingQuantity = slots[i].AddQuantity(remainingQuantity);
            }
        }

        // Bước 2: Nếu còn dư, thêm vào slot trống
        for (int i = 0; i < slots.Count && remainingQuantity > 0; i++)
        {
            if (slots[i].IsEmpty())
            {
                slots[i].itemData = itemData;
                slots[i].quantity = 0;
                remainingQuantity = slots[i].AddQuantity(remainingQuantity);
            }
        }

        // Kiểm tra có thêm được hết không
        bool success = remainingQuantity == 0;

        if (success)
        {
            Debug.Log($"Added {quantity}x {itemData.itemName} to inventory");
            onInventoryChanged?.Invoke();
        }
        else
        {
            Debug.LogWarning($"Inventory full! Could not add {remainingQuantity}x {itemData.itemName}");
            // Vẫn thêm được một phần
            if (remainingQuantity < quantity)
            {
                onInventoryChanged?.Invoke();
            }
        }
        return success;
    }

    // Xóa item khỏi inventory
    public bool RemoveItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null || quantity <= 0)
        {
            Debug.LogWarning("Cannot remove invalid item or quantity");
            return false;
        }

        // Kiểm tra có đủ item không
        if (!HasItem(itemData, quantity))
        {
            Debug.LogWarning($"Not enough {itemData.itemName} in inventory");
            return false;
        }

        int remainingToRemove = quantity;

        // Xóa item từ các slot
        for (int i =0; i < slots.Count && remainingToRemove > 0; i++)
        {
            if (!slots[i].IsEmpty() && slots[i].itemData == itemData)
            {
                int removeAmount = Mathf.Min(slots[i].quantity, remainingToRemove);
                slots[i].RemoveQuantity(removeAmount);
                remainingToRemove -= removeAmount;
            }
        }

        Debug.Log($"Removed {quantity}x {itemData.itemName} from inventory");
        onInventoryChanged?.Invoke();
        return true;
    }

    // Kiểm tra có item không (và số lượng)
    public bool HasItem(ItemData itemData, int quantity = 1)
    {
        if (itemData == null) return false;

        int totalCount = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.itemData == itemData)
            {
                totalCount += slot.quantity;
                if (totalCount >= quantity)
                return true;
            }
        }

        return false;
    }

    // Đếm tổng số lượng của 1 item
    public int GetItemCount(ItemData itemData)
    {
        if (itemData == null) return 0;
        int count = 0;
        foreach (var slot in slots)
        {
            if (!slot.IsEmpty() && slot.itemData == itemData)
            {
                count += slot.quantity;
            }
        }
        return count;
    }

    // Lấy danh sách tất cả slots (dùng cho UI)
    public List<InventorySlot> GetSlots()
    {
        return slots;
    }

    // Lấy slot theo index
    public InventorySlot GetSlot(int index)
    {
        if (index >= 0 && index < slots.Count)
            return slots[index];
        return null;
    }

    // Xóa toàn bộ inventory
    public void ClearInventory()
    {
        foreach (var slot in slots)
        {
            slot.Clear();
        }
        Debug.Log("Inventory cleared");
        onInventoryChanged?.Invoke();
    }

    // Kiểm tra inventory có đầy không
    public bool IsFull()
    {
        foreach (var slot in slots)
        {
            if (slot.IsEmpty())
                return false;
        }

        return true;
    }

    // Debug: Hiển thị toàn bộ inventory
    public void PrintInventory()
    {
        Debug.Log("===INVENTORY CONTENTS ===");
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsEmpty())
            {
                Debug.Log($"Slot {i}: {slots[i].itemData.itemName} x{slots[i].quantity}");
            }
        }
        Debug.Log("========================");
    }

    public List<InventorySlot> GetAllSlots()
    {
        return slots;
    }
}
