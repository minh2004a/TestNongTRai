using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject slotPrefab;
    [SerializeField] private Transform slotsParent; // InventoryPanel
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Settings")]
    [SerializeField] private int numberOfSlots = 20;

    private List<SlotUI> slots = new List<SlotUI>();

    [SerializeField] public GameObject inventoryPanel;

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

        InitializeSlots();
        RefreshUI();

        // Subscribe vào event khi inventory thay đổi
        if (inventoryManager != null)
        {
            inventoryManager.onInventoryChanged += RefreshUI;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            this.ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (!inventoryPanel.activeSelf)
        {
            inventoryPanel.SetActive(true);
        }
        else
        {
            inventoryPanel.SetActive(false);
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

    /// <summary>
    /// Tạo các slot UI
    /// </summary>
    private void InitializeSlots()
    {
        // Clear các slot cũ nếu có
        foreach (Transform child in slotsParent)
        {
            Destroy(child.gameObject);
        }
        slots.Clear();

        // Tạo số lượng slots theo setting
        for (int i = 0; i < numberOfSlots; i++)
        {
            GameObject slotObj = Instantiate(slotPrefab, slotsParent);
            SlotUI slotUI = slotObj.GetComponent<SlotUI>();

            if (slotUI == null)
            {
                slotUI = slotObj.AddComponent<SlotUI>();
            }

            slots.Add(slotUI);
        }

        Debug.Log($"Đã tạo {numberOfSlots} slots UI");
    }

    /// <summary>
    /// Cập nhật toàn bộ UI từ dữ liệu inventory
    /// </summary>
    public void RefreshUI()
    {
        if (inventoryManager == null) return;

        var slots = inventoryManager.GetAllSlots();

        for (int i = 0; i < this.slots.Count; i++)
        {
            if (i < slots.Count)
            {
                this.slots[i].SetSlot(slots[i]);
            }
            else
            {
                this.slots[i].ClearSlot();
            }
        }
    }

    /// <summary>
    /// Toggle hiển thị inventory (để bind với phím tắt)
    /// </summary>
    //public void ToggleInventory()
    //{
    //    slotsParent.gameObject.SetActive(!slotsParent.gameObject.activeSelf);
    //}
}
