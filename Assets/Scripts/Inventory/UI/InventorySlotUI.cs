using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using TinyFarm.Items;
using TinyFarm.Items.UI;

namespace TinyFarm.NPC
{
    /// <summary>
    /// Slot hiển thị item trong inventory người chơi khi mở Shop
    /// Dùng riêng cho ShopUI (không drag/drop)
    /// </summary>
    public class InventorySlotUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("UI References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Image backgroundImage;

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = new Color(0.9f, 0.9f, 1f);
        [SerializeField] private Color emptyColor = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField] private float hoverScale = 1.05f;
        [SerializeField] private float animationSpeed = 10f;

        // Runtime
        private InventorySlot slotData;
        private bool isHovered = false;
        private Vector3 originalScale;

        // Event callback cho ShopUI
        public event Action<InventorySlot> OnSlotClicked;

        public bool IsEmpty => slotData == null || slotData.IsEmpty;

        private void Awake()
        {
            originalScale = transform.localScale;

            if (itemIcon != null) itemIcon.enabled = false;
            if (quantityText != null) quantityText.enabled = false;
        }

        private void Update()
        {
            Vector3 targetScale = isHovered ? originalScale * hoverScale : originalScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
        }

        /// <summary>
        /// Gán dữ liệu của slot từ InventoryManager
        /// </summary>
        public void SetSlot(InventorySlot slot)
        {
            slotData = slot;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (slotData == null || slotData.IsEmpty)
            {
                itemIcon.enabled = false;
                quantityText.enabled = false;
                backgroundImage.color = emptyColor;
                return;
            }

            itemIcon.enabled = true;
            itemIcon.sprite = slotData.ItemIcon;

            if (slotData.Quantity > 1)
            {
                quantityText.enabled = true;
                quantityText.text = slotData.Quantity.ToString();
            }
            else
            {
                quantityText.enabled = false;
            }

            backgroundImage.color = normalColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsEmpty) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnSlotClicked?.Invoke(slotData);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            backgroundImage.color = hoverColor;

            // Nếu có tooltip hệ thống
            if (!IsEmpty && TooltipSystem.Instance != null)
            {
                TooltipSystem.Instance.ShowTooltip(slotData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            backgroundImage.color = normalColor;

            if (TooltipSystem.Instance != null)
            {
                TooltipSystem.Instance.HideTooltip();
            }
        }
    }
}
