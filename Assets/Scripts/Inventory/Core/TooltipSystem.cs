using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;

namespace TinyFarm.Items.UI
{
    public class TooltipSystem : MonoBehaviour
    {
        private static TooltipSystem instance;
        public static TooltipSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<TooltipSystem>();
                }
                return instance;
            }
        }

        [Header("References")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Image itemIconImage;

        [Header("Settings")]
        [SerializeField] private Vector2 offset = new Vector2(10, -10);
        [SerializeField] private float showDelay = 0.5f;

        // State
        private RectTransform tooltipRect;
        private float showTimer;
        private bool isShowing;
        private SlotUI currentSlot;
        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            tooltipRect = tooltipPanel.GetComponent<RectTransform>();
            HideTooltip();
        }

        // Show tooltip cho slot
        public void ShowTooltip(SlotUI slotUI)
        {
            if (slotUI == null || slotUI.IsEmpty)
            {
                HideTooltip();
                return;
            }

            currentSlot = slotUI;
            showTimer = 0f;
            isShowing = true;
        }

        /// Show tooltip ngay lập tức
        public void ShowTooltipImmediate(SlotUI slotUI)
        {
            ShowTooltip(slotUI);
            showTimer = showDelay;
            UpdateTooltip();
        }

        /// Hide tooltip
        public void HideTooltip()
        {
            isShowing = false;
            currentSlot = null;
            tooltipPanel.SetActive(false);
        }

        private void Update()
        {
            if (!isShowing) return;

            // Delay before showing
            showTimer += Time.deltaTime;

            if (showTimer >= showDelay)
            {
                UpdateTooltip();
            }

            // Update position to follow mouse
            UpdatePosition();
        }

        private void UpdateTooltip()
        {
            if (currentSlot == null || currentSlot.IsEmpty)
            {
                HideTooltip();
                return;
            }

            tooltipPanel.SetActive(true);

            Item item = currentSlot.Slot.Item;
            UpdateTooltipContent(item);
        }

        private void UpdateTooltipContent(Item item)
        {
            if (item == null) return;

            // Item name
            if (itemNameText != null)
            {
                itemNameText.text = item.Name;
                itemNameText.color = GetRarityColor(item.materialTier);
            }

            // Description
            if (descriptionText != null)
            {
                descriptionText.text = item.Description;
            }

            // Stats
            if (statsText != null)
            {
                statsText.text = GetStatsText(item);
            }

            // Icon
            if (itemIconImage != null)
            {
                itemIconImage.sprite = item.Icon;
            }
        }

        private string GetStatsText(Item item)
        {
            string stats = "";

            // Stack info
            if (item.IsStackable)
            {
                stats += $"Stack: {item.CurrentStack}/{item.Stackable.MaxStackSize}\n";
            }

            // Durability info
            if (item.HasDurability)
            {
                stats += $"Durability: {item.Durability.CurrentDurability:F0}/{item.Durability.MaxDurability}\n";
            }

            // Item type
            stats += $"\nType: {item.ItemData.GetItemType()}";

            // Material tier
            stats += $"\nTier: {item.materialTier}";

            return stats;
        }

        private Color GetRarityColor(MaterialTier tier)
        {
            return tier switch
            {
                MaterialTier.Common => Color.white,
                MaterialTier.Rare => Color.blue,
                MaterialTier.Epic => new Color(0.6f, 0f, 1f), // Purple
                MaterialTier.Legendary => new Color(1f, 0.5f, 0f), // Orange
                _ => Color.white
            };
        }

        private void UpdatePosition()
        {
            if (tooltipRect == null) return;

            Vector2 mousePosition = Input.mousePosition;
            tooltipRect.position = mousePosition + offset;

            // Clamp to screen
            Vector3[] corners = new Vector3[4];
            tooltipRect.GetWorldCorners(corners);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            // Check right edge
            if (corners[2].x > screenWidth)
            {
                tooltipRect.position = new Vector2(
                    mousePosition.x - tooltipRect.sizeDelta.x - offset.x,
                    tooltipRect.position.y
                );
            }

            // Check bottom edge
            if (corners[0].y < 0)
            {
                tooltipRect.position = new Vector2(
                    tooltipRect.position.x,
                    mousePosition.y + tooltipRect.sizeDelta.y - offset.y
                );
            }

        }
    }
}


