using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;


namespace TinyFarm.Items.UI
{
    public class HotbarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private HotbarSystem hotbarSystem;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotUIPrefab;

        private List<SlotUI> hotbarSlotUIs = new List<SlotUI>();

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Tìm HotbarSystem nếu chưa được gán
            if (hotbarSystem == null)
            {
                hotbarSystem = FindObjectOfType<HotbarSystem>();
                if (hotbarSystem == null)
                {
                    Debug.LogError("[HotbarUI] Không tìm thấy HotbarSystem!");
                    return;
                }
            }

            // Đảm bảo HotbarSystem đã được khởi tạo
            if (!hotbarSystem.IsInitialized)
            {
                Debug.LogWarning("[HotbarUI] HotbarSystem chưa khởi tạo. Đang chờ...");
                hotbarSystem.OnHotbarInitialized += CreateHotbarSlots;
            }
            else
            {
                CreateHotbarSlots();
            }

            // Đăng ký sự kiện
            hotbarSystem.OnSlotSelectionChanged += OnHotbarSlotSelectionChanged;
        }

        private void Update()
        {
            HandleInput();
        }

        private void CreateHotbarSlots()
        {
            // Hủy đăng ký để tránh gọi lại
            hotbarSystem.OnHotbarInitialized -= CreateHotbarSlots;

            int hotbarSize = hotbarSystem.HotbarSize;

            // Xóa các Slot cũ nếu có
            foreach (var slotUI in hotbarSlotUIs)
            {
                Destroy(slotUI.gameObject);
            }
            hotbarSlotUIs.Clear();

            // Tạo các Slot UI mới
            for (int i = 0; i < hotbarSize; i++)
            {
                GameObject slotObject = Instantiate(slotUIPrefab, slotsContainer);
                SlotUI slotUI = slotObject.GetComponent<SlotUI>();

                if (slotUI == null)
                {
                    Debug.LogError("[HotbarUI] Slot UI Prefab thiếu component SlotUI!");
                    Destroy(slotObject);
                    continue;
                }

                // Lấy InventorySlot tương ứng từ HotbarSystem
                InventorySlot inventorySlot = hotbarSystem.GetHotbarSlot(i);

                // Gán InventorySlot vào SlotUI
                slotUI.Initialize(inventorySlot);

                // Đăng ký sự kiện Click để chọn Slot
                slotUI.OnSlotClicked += OnHotbarSlotUIClicked;

                hotbarSlotUIs.Add(slotUI);
            }

            // Highlight Slot đã chọn ban đầu
            if (hotbarSlotUIs.Count > 0)
            {
                hotbarSlotUIs[hotbarSystem.SelectedSlotIndex].Select();
            }
        }

        private void HandleInput()
        {
            // Xử lý phím số 1-9
            for (int i = 0; i < hotbarSystem.HotbarSize; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                {
                    hotbarSystem.SelectSlot(i);
                    return;
                }
            }

            // Xử lý phím dùng vật phẩm (Giả định phím Space hoặc E)
            if (Input.GetKeyDown(KeyCode.F)) // Ví dụ: Sử dụng item khi nhấn Space
            {
                hotbarSystem.UseSelectedItem();
            }
        }

        private void OnHotbarSlotUIClicked(SlotUI slotUI)
        {
            // Tìm index của SlotUI được click và thông báo cho HotbarSystem
            int index = hotbarSlotUIs.IndexOf(slotUI);
            if (index != -1)
            {
                hotbarSystem.SelectSlot(index);
            }
        }

        private void OnHotbarSlotSelectionChanged(int oldIndex, int newIndex)
        {
            // Bỏ highlight Slot cũ
            if (oldIndex >= 0 && oldIndex < hotbarSlotUIs.Count)
            {
                hotbarSlotUIs[oldIndex].Deselect();
            }

            // Highlight Slot mới
            if (newIndex >= 0 && newIndex < hotbarSlotUIs.Count)
            {
                hotbarSlotUIs[newIndex].Select();
            }
        }

        private void OnDestroy()
        {
            if (hotbarSystem != null)
            {
                hotbarSystem.OnSlotSelectionChanged -= OnHotbarSlotSelectionChanged;
            }

            foreach (var slotUI in hotbarSlotUIs)
            {
                if (slotUI != null)
                {
                    slotUI.OnSlotClicked -= OnHotbarSlotUIClicked;
                    // Hủy đăng ký các sự kiện khác nếu cần
                }
            }
        }
    }
}

