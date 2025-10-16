using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;


namespace TinyFarm.Items.UI
{
    public class InventoryDescription : MonoBehaviour
    {
        [Header("UI References")]
        public TextMeshProUGUI itemNameText;
        public TextMeshProUGUI itemTypeText;
        public TextMeshProUGUI descriptionText;
        public GameObject separator;

        [Header("Settings")]
        public Color normalTextColor = new Color(0.2f, 0.1f, 0.05f); // Dark brown
        public Color typeTextColor = Color.gray;

        private void Start()
        {
            // Hiển thị trống khi bắt đầu
            Clear();
        }

        public void ShowItemInfo(ItemData itemData)
        {
            if (itemData == null)
            {
                Clear();
                return;
            }

            // Hiển thị tên
            itemNameText.text = itemData.itemName;

            // Hiển thị type
            itemTypeText.text = GetItemTypeText(itemData.itemType);
            itemTypeText.color = typeTextColor;

            // Hiển thị description
            descriptionText.text = itemData.description;
            descriptionText.color = normalTextColor;

            // Hiện separator
            if (separator != null)
                separator.SetActive(true);
        }

        public void Clear()
        {
            itemNameText.text = "";
            itemTypeText.text = "";
            descriptionText.text = "";

            if (separator != null)
                separator.SetActive(false);
        }

        private string GetItemTypeText(ItemType type)
        {
            switch (type)
            {
                case ItemType.Tool: return "Tool";
                case ItemType.Equipment: return "Weapon";
                case ItemType.Consumable: return "Consumable";
                case ItemType.Seed: return "Seed";
                case ItemType.Crop: return "Crop";
                default: return "Item";
            }
        }

        
    }
}

