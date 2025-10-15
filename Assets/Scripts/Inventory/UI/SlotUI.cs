using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;

namespace TinyFarm.Items.UI
{
    public class SlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image highlightImage;
        [SerializeField] private GameObject lockedOverlay;

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Color selectedColor = Color.green;
        [SerializeField] private Color lockedColor = Color.gray;

        [Header("Animation")]
        [SerializeField] private bool enableHoverAnimation = true;
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float animationSpeed = 10f;

        // State
        private InventorySlot slot;
        private bool isHovered = false;
        private bool isSelected = false;
        private Vector3 originalScale;

        // Events
        public event Action<SlotUI> OnSlotClicked;
        public event Action<SlotUI> OnSlotRightClicked;
        public event Action<SlotUI> OnSlotHoverEnter;
        public event Action<SlotUI> OnSlotHoverExit;

        // Properties
        public InventorySlot Slot => slot;
        public int SlotIndex => slot?.SlotIndex ?? -1;
        public bool IsEmpty => slot?.IsEmpty ?? true;
        public bool IsHovered => isHovered;
        public bool IsSelected => isSelected;

        private void Awake()
        {
            originalScale = transform.localScale;
            ValidateReferences();
        }

        private void ValidateReferences()
        {
            if (itemIcon == null)
                Debug.LogError("[SlotUI] ItemIcon not assigned!", this);

            if (quantityText == null)
                Debug.LogWarning("[SlotUI] QuantityText not assigned!", this);

            if (backgroundImage == null)
                Debug.LogWarning("[SlotUI] BackgroundImage not assigned!", this);
        }

        /// Bind slot data với UI
        public void Initialize(InventorySlot inventorySlot)
        {
            if (inventorySlot == null)
            {
                Debug.LogError("[SlotUI] Cannot initialize with null slot!");
                return;
            }

            // Unsubscribe old slot nếu có
            if (slot != null)
            {
                UnsubscribeFromSlot();
            }

            slot = inventorySlot;
            SubscribeToSlot();

            // Initial update
            UpdateUI();
        }

        private void SubscribeToSlot()
        {
            if (slot == null) return;

            slot.OnSlotChanged += HandleSlotChanged;
            slot.OnSlotLockChanged += HandleLockChanged;
        }

        private void UnsubscribeFromSlot()
        {
            if (slot == null) return;

            slot.OnSlotChanged -= HandleSlotChanged;
            slot.OnSlotLockChanged -= HandleLockChanged;
        }

        // Update toàn bộ UI dựa vào slot data
        public void UpdateUI()
        {
            if (slot == null) return;

            UpdateIcon();
            UpdateQuantity();
            UpdateBackground();
            UpdateLockedState();
        }

        private void UpdateIcon()
        {
            if (itemIcon == null) return;

            if (slot.IsEmpty)
            {
                itemIcon.sprite = null;
                itemIcon.enabled = false;
            }
            else
            {
                itemIcon.sprite = slot.ItemIcon;
                itemIcon.enabled = true;
            }
        }

        private void UpdateQuantity()
        {
            if (quantityText == null) return;

            if (slot.IsEmpty || slot.Quantity <= 1)
            {
                quantityText.text = "";
                quantityText.enabled = false;
            }
            else
            {
                quantityText.text = slot.Quantity.ToString();
                quantityText.enabled = true;
            }
        }

        private void UpdateBackground()
        {
            if (backgroundImage == null) return;

            if (isSelected)
            {
                backgroundImage.color = selectedColor;
            }
            else if (slot.IsLocked)
            {
                backgroundImage.color = lockedColor;
            }
            else
            {
                backgroundImage.color = normalColor;
            }
        }

        private void UpdateLockedState()
        {
            if (lockedOverlay != null)
            {
                lockedOverlay.SetActive(slot.IsLocked);
            }
        }

        public void Select()
        {
            isSelected = true;
            UpdateBackground();

            if (highlightImage != null)
            {
                highlightImage.enabled = true;
                highlightImage.color = selectedColor;
            }
        }

        public void Deselect()
        {
            isSelected = false;
            UpdateBackground();

            if (highlightImage != null && !isHovered)
            {
                highlightImage.enabled = false;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;

            if (highlightImage != null && !isSelected)
            {
                highlightImage.enabled = true;
                highlightImage.color = highlightColor;
            }

            OnSlotHoverEnter?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;

            if (highlightImage != null && !isSelected)
            {
                highlightImage.enabled = false;
            }

            OnSlotHoverExit?.Invoke(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (slot?.IsLocked ?? true) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnSlotClicked?.Invoke(this);
            }
            else if (eventData.button == PointerEventData.InputButton.Right)
            {
                OnSlotRightClicked?.Invoke(this);
            }
        }

        private void Update()
        {
            if (!enableHoverAnimation) return;

            Vector3 targetScale = isHovered ? originalScale * hoverScale : originalScale;
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
        }

        private void HandleSlotChanged(InventorySlot changedSlot)
        {
            UpdateUI();
        }

        private void HandleLockChanged(bool locked)
        {
            UpdateLockedState();
            UpdateBackground();
        }

        private void OnDestroy()
        {
            UnsubscribeFromSlot();
        }
    }
}
