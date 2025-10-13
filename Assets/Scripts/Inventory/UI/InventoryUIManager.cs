using System.Collections.Generic;
using UnityEngine;
using TinyFarm.Items;

public class InventoryUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotsParent;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Settings")]
    [SerializeField] private int numberOfSlots = 20;

    private List<SlotUI> slotUIList = new List<SlotUI>(); // Đổi tên tránh conflict

    private void Start()
    {
        // Tự động tìm InventoryManager nếu chưa gán
        if (inventoryManager == null)
        {
            inventoryManager = FindObjectOfType<InventoryManager>();
            if (inventoryManager == null)
            {
                Debug.LogError("Không tìm thấy InventoryManager!");
                return;
            }
        }

        // Ẩn inventory khi bắt đầu
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }

        // Khởi tạo slots
        InitializeSlots();

        // ⭐ CHỈ subscribe 1 lần duy nhất
        if (inventoryManager != null)
        {
            inventoryManager.onInventoryChanged += RefreshUI;
        }

        // Refresh UI lần đầu
        RefreshUI();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleInventory();
        }
    }

    private void OnDestroy()
    {
        // Unsubscribe để tránh memory leak
        if (inventoryManager != null)
        {
            inventoryManager.onInventoryChanged -= RefreshUI;
        }
    }

    /// Tạo các slot UI
    private void InitializeSlots()
    {
        // Clear các slot cũ nếu có
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }
        slotUIList.Clear();

        // Tạo số lượng slots theo setting
        for (int i = 0; i < numberOfSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            SlotUI slotUI = slotObj.GetComponent<SlotUI>();

            if (slotUI == null)
            {
                Debug.LogWarning($"Slot {i} không có SlotUI component! Đang thêm...");
                slotUI = slotObj.AddComponent<SlotUI>();
            }

            slotUIList.Add(slotUI);
        }

        Debug.Log($"Đã tạo {numberOfSlots} slots UI");
    }

    /// Cập nhật toàn bộ UI từ dữ liệu inventory
    public void RefreshUI()
    {
        if (inventoryManager == null) return;

        // ⭐ Đổi tên biến local để tránh conflict
        List<InventorySlot> inventorySlots = inventoryManager.GetAllSlots();

        for (int i = 0; i < slotUIList.Count; i++)
        {
            if (i < inventorySlots.Count)
            {
                slotUIList[i].SetSlot(inventorySlots[i]);
            }
            else
            {
                slotUIList[i].ClearSlot();
            }
        }
    }

    /// Toggle hiển thị inventory
    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool isActive = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(isActive);
            Debug.Log($"Inventory: {(isActive ? "Mở" : "Đóng")}");
        }
    }

    /// Mở inventory
    public void OpenInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(true);
        }
    }

    /// Đóng inventory
    public void CloseInventory()
    {
        if (inventoryPanel != null)
        {
            inventoryPanel.SetActive(false);
        }
    }
}