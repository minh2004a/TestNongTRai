using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TinyFarm.Items.UI
{
    public class EquipmentSlotUI : MonoBehaviour, 
    IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IDropHandler // Để drag & drop
    {
        [Header("References")]
        public Image backgroundImage;
        public Image itemIcon;
        public Image emptySlotIcon; // Icon mờ khi trống

        [Header("Settings")]
        public EquipmentSlotType slotType;

        [Header("Visual Settings")]
        public Color normalColor = Color.white;
        public Color hoverColor = new Color(1f, 1f, 0.8f);

        private EquipmentItemData equippedItem;
        private InventoryDescription descriptionPanel;

        private void Start()
        {
            Setup(slotType);
        }

        public void Setup(EquipmentSlotType type)
        {
            slotType = type;
            descriptionPanel = FindObjectOfType<InventoryDescription>();
            if (descriptionPanel == null)
            {
                Debug.LogWarning($"[EquipmentSlotUI] InventoryDescription not found!");
            }
            UpdateDisplay();
        }

        public void SetItem(EquipmentItemData item)
        {
            equippedItem = item;
            UpdateDisplay();
        }

        // Equip item vào slot này
        public void EquipItem(EquipmentItemData item)
        {
            if (item == null)
            {
                Debug.LogWarning("[EquipmentSlotUI] Cannot equip null item!");
                return;
            }

            // Unequip old item nếu có
            if (equippedItem != null)
            {
                UnequipItem();
            }

            equippedItem = item;
            UpdateDisplay();

            Debug.Log($"[EquipmentSlotUI] Equipped {item.itemName} in {slotType} slot");
        }


        private void UpdateDisplay()
        {
            if (equippedItem != null)
            {
                // Show item icon
                if (itemIcon != null)
                {
                    itemIcon.enabled = true;
                    itemIcon.sprite = equippedItem.icon;
                }

                // Hide empty slot icon
                if (emptySlotIcon != null)
                    emptySlotIcon.enabled = false;
            }
            else
            {
                // Hide item icon
                if (itemIcon != null)
                    itemIcon.enabled = false;

                // Show empty slot icon
                if (emptySlotIcon != null)
                    emptySlotIcon.enabled = true;
            }
        }

        private void UnequipItem()
        {
            if (equippedItem == null) return;

            // Tạo Item đúng cách
            EquipmentItem item = new EquipmentItem(equippedItem);

            // Add back to inventory
            bool added = InventoryManager.Instance.AddItem(item, 1);

            if (added)
            {
                // Clear slot
                equippedItem = null;
                UpdateDisplay();

                // Clear description
                if (descriptionPanel != null)
                    descriptionPanel.Clear();

                Debug.Log($"[EquipmentSlotUI] Unequipped from {slotType} slot");
            }
            else
            {
                Debug.LogWarning("[EquipmentSlotUI] Cannot unequip - inventory full!");
            }
        }

        // Clear slot (không return về inventory)
        public void ClearSlot()
        {
            equippedItem = null;
            UpdateDisplay();
        }

        // Get equipped item
        public EquipmentItemData GetEquippedItem()
        {
            return equippedItem;
        }

        // Check if slot is empty
        public bool IsEmpty()
        {
            return equippedItem == null;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Show item info in description panel
            if (equippedItem != null && descriptionPanel != null)
            {
                descriptionPanel.ShowItemInfo(equippedItem);
            }

            // Visual feedback
            if (backgroundImage != null)
                backgroundImage.color = hoverColor;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            // Clear description
            if (descriptionPanel != null)
                descriptionPanel.Clear();

            // Reset visual
            if (backgroundImage != null)
                backgroundImage.color = normalColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Right click to unequip
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                if (equippedItem != null)
                {
                    UnequipItem();
                }
            }
            // Left click - có thể dùng cho equip tooltip hoặc compare stats
            else if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (equippedItem != null)
                {
                    Debug.Log($"[EquipmentSlotUI] Clicked {equippedItem.itemName}");
                    // Có thể mở detailed info window
                }
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            // Get dragged object
            GameObject draggedObject = eventData.pointerDrag;

            if (draggedObject == null) return;

            // Try get SlotUI (from inventory)
            SlotUI slotUI = draggedObject.GetComponent<SlotUI>();

            if (slotUI != null)
            {
                InventorySlot inventorySlot = slotUI.Slot;

                if (inventorySlot != null && !inventorySlot.IsEmpty)
                {
                    // Try to equip item from inventory
                    bool success = InventoryManager.Instance.TryEquipItem(inventorySlot, this);

                    if (success)
                    {
                        Debug.Log($"[EquipmentSlotUI] Successfully equipped item via drag & drop");
                    }
                }
            }
        }

        [ContextMenu("Debug Equipment Slot")]
        private void DebugSlot()
        {
            Debug.Log($"=== EQUIPMENT SLOT ({slotType}) ===");
            Debug.Log($"Is Empty: {IsEmpty()}");

            if (equippedItem != null)
            {
                Debug.Log($"Equipped: {equippedItem.itemName}");
            }
        }
    }
}

