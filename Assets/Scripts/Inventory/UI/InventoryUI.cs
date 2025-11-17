using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using TinyFarm;
using TinyFarm.PlayerInput;
using System;
using System.Collections.Generic;

namespace TinyFarm.Items.UI
{
    public class InventoryUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private Transform slotsContainer;
        [SerializeField] private GameObject slotUIPrefab;
        private PlayerInputHandler inputHandler;

        [Header("UI Elements")]
        [SerializeField] private Button closeButton;

        [Header("Settings")]
        [SerializeField] private bool autoCreateSlots = true;

        // State
        private List<SlotUI> slotUIs = new List<SlotUI>();
        private SlotUI selectedSlot;
        private bool isOpen = false;

        // Events
        public event Action OnInventoryOpened;
        public event Action OnInventoryClosed;

        // Properties
        public bool IsOpen => isOpen;

        private void Start()
        {
            Initialize();
            inputHandler = FindObjectOfType<PlayerInputHandler>();
            if (inputHandler != null)
            {
                inputHandler.OnInventoryToggled += ToggleInventory;
            }
        }

        private void OnDestroy()
        {
            if (inputHandler != null)
            {
                inputHandler.OnInventoryToggled -= ToggleInventory;
            }

            foreach (var slotUI in slotUIs)
            {
                if (slotUI != null)
                {
                    slotUI.OnSlotClicked -= OnSlotUIClicked;
                    slotUI.OnSlotRightClicked -= OnSlotUIRightClicked;
                    slotUI.OnSlotHoverEnter -= OnSlotUIHoverEnter;
                    slotUI.OnSlotHoverExit -= OnSlotUIHoverExit;
                }
            }
        }
        
        private void Initialize()
        {
            // Find InventoryManager if not assigned
            if (inventoryManager == null)
            {
                inventoryManager = FindObjectOfType<InventoryManager>();
                if (inventoryManager == null)
                {
                    Debug.LogError("[InventoryUI] InventoryManager not found!");
                    return;
                }
            }


            // Setup buttons
            if (closeButton != null)
<<<<<<< HEAD
                closeButton.onClick.AddListener(() =>
                {
                    // Close đúng chuẩn bằng input handler
                    inputHandler?.SetInputState(InputState.Gameplay);
                });
<<<<<<< HEAD
=======
                closeButton.onClick.AddListener(CloseInventory);
>>>>>>> parent of 8fa43827 (Update InventoryUI.cs)

=======
>>>>>>> parent of 23a542d8 (Revert "Fix Button close Inventory")
            // Create slot UIs
            if (autoCreateSlots)
            {
                CreateSlotUIs();
            }

            // Initial state
            gameObject.SetActive(false);
            isOpen = false;
        }

        private void CreateSlotUIs()
        {
            if (slotUIPrefab == null)
            {
                Debug.LogError("[InventoryUI] SlotUI Prefab not assigned!");
                return;
            }

            if (slotsContainer == null)
            {
                Debug.LogError("[InventoryUI] Slots Container not assigned!");
                return;
            }

            // Clear existing slots
            foreach (Transform child in slotsContainer)
            {
                Destroy(child.gameObject);
            }
            slotUIs.Clear();

            // Create slot UIs
            var slots = inventoryManager.GetAllSlots();
            for (int i = 0; i < slots.Count; i++)
            {
                CreateSlotUI(slots[i]);
            }

        }

        private void CreateSlotUI(InventorySlot slot)
        {
            GameObject slotGO = Instantiate(slotUIPrefab, slotsContainer);
            SlotUI slotUI = slotGO.GetComponent<SlotUI>();

            if (slotUI != null)
            {
                slotUI.Initialize(slot);
                slotUI.OnSlotClicked += OnSlotUIClicked;
                slotUI.OnSlotRightClicked += OnSlotUIRightClicked;
                slotUI.OnSlotHoverEnter += OnSlotUIHoverEnter;
                slotUI.OnSlotHoverExit += OnSlotUIHoverExit;

                // Gán InventoryManager cho DragDropHandler
                var dragHandler = slotGO.GetComponent<DragDropHandler>();
                if (dragHandler != null)
                    dragHandler.inventoryManager = inventoryManager;

                slotUIs.Add(slotUI);
            }
        }

        private void Update()
        {
            
        }

        public void ToggleInventory()
        {
            if (isOpen)
            {
                CloseInventory();
            }
            else
            {
                OpenInventory();
            }
        }

        public void OpenInventory()
        {
            if (isOpen) return;

            gameObject.SetActive(true);
            isOpen = true;

            TinyFarm.GameplayBlocker.UIOpened = true;

            UpdateUI();
            OnInventoryOpened?.Invoke();
        }

        public void CloseInventory()
        {
            if (!isOpen) return;

            gameObject.SetActive(false);
            isOpen = false;

            TinyFarm.GameplayBlocker.UIOpened = false;
            DeselectAllSlots();
            TooltipSystem.Instance?.HideTooltip();

            OnInventoryClosed?.Invoke();
        }

        public void UpdateUI()
        {
            // Update all slot UIs
            foreach (var slotUI in slotUIs)
            {
                slotUI.UpdateUI();
            }
        }

        private void OnSlotUIClicked(SlotUI slotUI)
        {
            if (slotUI.IsEmpty) return;

            // Select/Deselect
            if (selectedSlot == slotUI)
            {
                DeselectSlot(slotUI);
            }
            else
            {
                SelectSlot(slotUI);
            }
        }

        private void OnSlotUIRightClicked(SlotUI slotUI)
        {
            if (slotUI.IsEmpty) return;

            // Use item
            slotUI.Slot.UseItem();
        }

        private void OnSlotUIHoverEnter(SlotUI slotUI)
        {
            // Show tooltip
            TooltipSystem.Instance?.ShowTooltip(slotUI);
        }

        private void OnSlotUIHoverExit(SlotUI slotUI)
        {
            // Hide tooltip
            TooltipSystem.Instance?.HideTooltip();
        }

        private void SelectSlot(SlotUI slotUI)
        {
            DeselectAllSlots();
            selectedSlot = slotUI;
            slotUI.Select();
        }

        private void DeselectSlot(SlotUI slotUI)
        {
            slotUI.Deselect();
            if (selectedSlot == slotUI)
            {
                selectedSlot = null;
            }
        }

        private void DeselectAllSlots()
        {
            foreach (var slotUI in slotUIs)
            {
                slotUI.Deselect();
            }
            selectedSlot = null;
        }

        private void OnSortButtonClicked()
        {
            inventoryManager.SortInventory();
            UpdateUI();
        }
        
    }
}
