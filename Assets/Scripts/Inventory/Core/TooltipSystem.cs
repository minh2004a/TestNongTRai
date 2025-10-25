using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using TinyFarm.PlayerInput;
using System.Collections.Generic;

namespace TinyFarm.Items.UI
{
    public class TooltipSystem : MonoBehaviour
    {
        public static TooltipSystem Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private RectTransform tooltipRectTransform;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Content References")]
        [SerializeField] private TextMeshProUGUI itemNameText;
        [SerializeField] private TextMeshProUGUI itemTypeText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private GameObject separator;

        [Header("Settings")]
        [SerializeField] private Vector2 offset = new Vector2(100f, 0f);
        [SerializeField] private float padding = 40f;
        [SerializeField] private float fadeSpeed = 100f;
        [SerializeField] private bool followCursor = true;
        [SerializeField] private Color normalTextColor = new Color(0.2f, 0.1f, 0.05f);
        [SerializeField] private Color typeTextColor = Color.gray;

        [SerializeField] private Canvas canvas;
        [SerializeField] private Camera mainCamera;
        private bool isVisible = false;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Initialize();
        }

        private void Initialize()
        {
            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }
            if (mainCamera  == null)
            {
                mainCamera = Camera.main;
            }

            if (tooltipPanel == null)
            {
                Debug.LogError("[TooltipSystem] Tooltip Panel not assigned!");
                return;
            }

            if (tooltipRectTransform == null)
                tooltipRectTransform = tooltipPanel.GetComponent<RectTransform>();

            if (canvasGroup == null)
                canvasGroup = tooltipPanel.GetComponent<CanvasGroup>();

            // Start hidden
            HideTooltip();
        }

        private void Update()
        {
            if (isVisible && followCursor)
            {
                UpdatePosition();
            }

            // Smooth fade in/out
            if (canvasGroup != null)
            {
                float targetAlpha = isVisible ? 1f : 0f;
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
            }
        }

        // Show tooltip for SlotUI
        public void ShowTooltip(SlotUI slotUI)
        {
            if (slotUI == null || slotUI.IsEmpty)
            {
                HideTooltip();
                return;
            }

            ShowTooltip(slotUI.Slot);
        }

        /// <summary>
        /// Show tooltip for HotBarSlotUI
        /// </summary>
        //public void ShowTooltip(HotBarSlotUI hotBarSlotUI)
        //{
        //    if (hotBarSlotUI == null || hotBarSlotUI.IsEmpty)
        //    {
        //        HideTooltip();
        //        return;
        //    }

        //    ShowTooltip(hotBarSlotUI.Slot);
        //}

        // Show tooltip for InventorySlot
        /// </summary>
        public void ShowTooltip(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty)
            {
                Debug.LogWarning("[TooltipSystem] Slot is null or empty");
                HideTooltip();
                return;
            }

            ItemData itemData = slot.ItemData;
            if (itemData == null)
            {
                Debug.LogWarning("[TooltipSystem] ItemData is null");
                HideTooltip();
                return;
            }

            ShowTooltip(itemData);
        }

        // Show tooltip for ItemData
        public void ShowTooltip(ItemData itemData)
        {
            if (itemData == null)
            {
                HideTooltip();
                return;
            }

            // Set item name
            if (itemNameText != null)
            {
                itemNameText.text = itemData.itemName;
                itemNameText.color = normalTextColor;
            }

            // Set item type
            if (itemTypeText != null)
            {
                itemTypeText.text = GetItemTypeText(itemData.itemType);
                itemTypeText.color = typeTextColor;
            }

            // Set description
            if (descriptionText != null)
            {
                descriptionText.text = itemData.description;
                descriptionText.color = normalTextColor;
            }

            // Show separator
            if (separator != null)
                separator.SetActive(true);

            // Show tooltip
            tooltipPanel.SetActive(true);
            isVisible = true;

            UpdatePosition();

            // Force rebuild layout để tooltip size đúng
            LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipRectTransform);
        }

        public void HideTooltip()
        {
            isVisible = false;

            if (canvasGroup != null && canvasGroup.alpha <= 0.01f)
            {
                tooltipPanel.SetActive(false);
            }
        }

        private void UpdatePosition()
        {
            if (tooltipRectTransform == null || canvas == null)
                return;

            Vector2 mousePosition = PlayerInput.mousePosition;

            // Convert mouse position to canvas space
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                mousePosition,
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out localPoint
            );

            // Get canvas rect and tooltip size
            RectTransform canvasRect = canvas.transform as RectTransform;
            Rect canvasPixelRect = canvasRect.rect;
            Vector2 tooltipSize = tooltipRectTransform.sizeDelta;

            // Calculate tooltip position with offset
            Vector2 tooltipPosition = localPoint + offset;

            // Smart positioning - flip if tooltip goes off screen
            // Check right edge
            if (tooltipPosition.x + tooltipSize.x > canvasPixelRect.width / 2 - padding)
            {
                tooltipPosition.x = localPoint.x - tooltipSize.x - offset.x;
            }

            // Check bottom edge
            if (tooltipPosition.y - tooltipSize.y < -canvasPixelRect.height / 2 + padding)
            {
                tooltipPosition.y = localPoint.y + tooltipSize.y - offset.y;
            }

            // Check left edge
            if (tooltipPosition.x < -canvasPixelRect.width / 2 + padding)
            {
                tooltipPosition.x = -canvasPixelRect.width / 2 + padding;
            }

            // Check top edge
            if (tooltipPosition.y > canvasPixelRect.height / 2 - padding)
            {
                tooltipPosition.y = canvasPixelRect.height / 2 - padding;
            }
            // Apply position smoothly
            if (fadeSpeed > 0)
            {
                tooltipRectTransform.anchoredPosition = Vector2.Lerp(
                    tooltipRectTransform.anchoredPosition,
                    tooltipPosition,
                    Time.deltaTime * fadeSpeed
                );
            }
            else
            {
                tooltipRectTransform.anchoredPosition = tooltipPosition;
            }
        }

        private string GetItemTypeText(ItemType type)
        {
            switch (type)
            {
                case ItemType.Tool: return "Tool";
                case ItemType.Equipment: return "Equipment";
                case ItemType.Consumable: return "Consumable";
                case ItemType.Seed: return "Seed";
                case ItemType.Crop: return "Crop";
                default: return "Item";
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}


