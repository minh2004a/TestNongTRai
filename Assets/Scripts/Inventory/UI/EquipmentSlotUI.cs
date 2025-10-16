using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TinyFarm.Items.UI
{
    public class EquipmentSlotUI : MonoBehaviour, IPointerClickHandler,
    IPointerEnterHandler,
    IPointerExitHandler,
    IDropHandler // Để drag & drop
    {
        [Header("References")]
        public Image itemIcon;
        public Image emptySlotIcon; // Icon mờ khi trống
        public Image highlightBorder;

        [Header("Settings")]
        public EquipmentSlotType slotType;

        private ItemData equippedItem;
        private InventoryDescription descriptionPanel;

        public void Setup(EquipmentSlotType type)
        {
            slotType = type;
            descriptionPanel = FindObjectOfType<InventoryDescription>();
            UpdateDisplay();
        }

        public void SetItem(ItemData item)
        {
            equippedItem = item;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (equippedItem != null)
            {
                itemIcon.enabled = true;
                itemIcon.sprite = equippedItem.icon;

                if (emptySlotIcon != null)
                    emptySlotIcon.enabled = false;
            }
            else
            {
                itemIcon.enabled = false;

                if (emptySlotIcon != null)
                    emptySlotIcon.enabled = true;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (equippedItem != null && descriptionPanel != null)
            {
                descriptionPanel.ShowItemInfo(equippedItem);
            }

            if (highlightBorder != null)
                highlightBorder.enabled = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (descriptionPanel != null)
                descriptionPanel.Clear();

            if (highlightBorder != null)
                highlightBorder.enabled = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (equippedItem != null)
            {
                // Unequip item - return to inventory
                UnequipItem();
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            // Handle drag & drop từ inventory vào equipment slot
            // Code này sẽ viết sau khi làm drag & drop system
        }

        private void UnequipItem()
        {
            if (equippedItem == null) return;

            // Add back to inventory
            InventoryManager.Instance.AddItem(new EquipmentItem((EquipmentItemData)equippedItem), 1);

            // Clear slot
            equippedItem = null;
            UpdateDisplay();

            // Clear description
            if (descriptionPanel != null)
                descriptionPanel.Clear();
        }
    }
}

