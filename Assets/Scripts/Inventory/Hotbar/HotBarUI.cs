using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

namespace TinyFarm.Items.UI
{
    public class HotBarUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject hotBarSlotPrefab;

        [Header("Settings")]
        [SerializeField] private int hotBarSize = 10; // 0-9
        [SerializeField] private bool autoCreateSlots = true;
        [SerializeField]

        // State
        private List<HotbarSlotUI> slotUIs = new List<HotbarSlotUI>();
        private int selectedSlotIndex = 0;

        // Events
        public event Action<int> OnSlotSelected;

        // Properties
        public int SelectedSlotIndex => selectedSlotIndex;
        public HotbarSlotUI SelectedSlot => selectedSlotIndex >= 0 && selectedSlotIndex < slotUIs.Count
            ? slotUIs[selectedSlotIndex]
            : null;

        private void Start()
        {
            Initialize();
        }

        private void Initialize()
        {
            // Find InventoryManager if not assigned
            if (inventoryManager == null)
            {
                inventoryManager = FindObjectOfType<InventoryManager>();
                if (inventoryManager == null)
                {
                    Debug.LogError("[HotBarUI] InventoryManager not found!");
                    return;
                }
            }

            // Create slot UIs
            if (autoCreateSlots)
            {
                CreateSlotUIs();
            }

            // Select first slot by default
            SelectSlot(0);
        }

        private void CreateSlotUIs()
        {
            if (hotBarSlotPrefab == null)
            {
                Debug.LogError("[HotBarUI] HotBarSlot Prefab not assigned!");
                return;
            }

            if (slotsContainer == null)
            {
                Debug.LogError("[HotBarUI] Slots Container not assigned!");
                return;
            }

            // Clear existing slots
            foreach (Transform child in slotsContainer)
            {
                Destroy(child.gameObject);
            }
            slotUIs.Clear();

            // Create hotbar slot UIs (first 10 slots from inventory)
            var slots = inventoryManager.GetAllSlots();
            for (int i = 0; i < Mathf.Min(hotBarSize, slots.Count); i++)
            {
                CreateSlotUI(slots[i], i);
            }

        }

        private void CreateSlotUI(InventorySlot slot, int index)
        {
            GameObject slotGO = Instantiate(hotBarSlotPrefab, slotsContainer);
            HotbarSlotUI slotUI = slotGO.GetComponent<HotbarSlotUI>();

            if (slotUI != null)
            {
                slotUI.Initialize(slot, index);
                slotUI.OnSlotClicked += OnSlotUIClicked;
                slotUI.OnSlotRightClicked += OnSlotUIRightClicked;
                slotUI.OnSlotHoverExit += OnSlotUIHoverExit;

                slotUIs.Add(slotUI);
            }
        }

        public void SelectSlot(int index)
        {
            if (index < 0 || index >= slotUIs.Count)
            {
                Debug.LogWarning($"[HotBarUI] Invalid slot index: {index}");
                return;
            }

            // Deselect old slot
            if (selectedSlotIndex >= 0 && selectedSlotIndex < slotUIs.Count)
            {
                slotUIs[selectedSlotIndex].Deselect();
            }

            // Select new slot
            selectedSlotIndex = index;
            slotUIs[selectedSlotIndex].Select();

            OnSlotSelected?.Invoke(selectedSlotIndex);
        }

        public void UseSelectedSlot()
        {
            var slot = SelectedSlot;
            if (slot != null && !slot.IsEmpty)
            {
                slot.Slot.UseItem();
                Debug.Log($"[HotBarUI] Used item from slot {selectedSlotIndex}");
            }
        }

        public void UpdateUI()
        {
            // Update all slot UIs
            foreach (var slotUI in slotUIs)
            {
                slotUI.UpdateUI();
            }
        }

        private void OnSlotUIClicked(HotbarSlotUI slotUI)
        {
            // Select clicked slot
            int index = slotUIs.IndexOf(slotUI);
            if (index >= 0)
            {
                SelectSlot(index);
            }
        }

        private void OnSlotUIRightClicked(HotbarSlotUI slotUI)
        {
            if (slotUI.IsEmpty) return;

            // Use item
            slotUI.Slot.UseItem();
        }

        private void OnSlotUIHoverExit(HotbarSlotUI slotUI)
        {
            // Hide tooltip
            TooltipSystem.Instance?.HideTooltip();
        }

        private void OnDestroy()
        {
            foreach (var slotUI in slotUIs)
            {
                if (slotUI != null)
                {
                    slotUI.OnSlotClicked -= OnSlotUIClicked;
                    slotUI.OnSlotRightClicked -= OnSlotUIRightClicked;
                    slotUI.OnSlotHoverExit -= OnSlotUIHoverExit;
                }
            }
        }
    }
}

