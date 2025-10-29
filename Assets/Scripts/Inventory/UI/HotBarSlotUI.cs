using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;

namespace TinyFarm.Items.UI
{
    public class HotbarSlotUI : MonoBehaviour,
        ISlotUIBase,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerClickHandler
    {
        [Header("References")]
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private TextMeshProUGUI keyNumberText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image selectionBorder;
        [SerializeField] private GameObject lockedOverlay;

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color highlightColor = Color.yellow;
        [SerializeField] private Color lockedColor = Color.gray;
        [SerializeField] private Color borderColor = new Color(1f, 0.8f, 0f, 1f); // Gold color

        [Header("Animation")]
        [SerializeField] private bool enableHoverAnimation = true;
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float animationSpeed = 10f;

        // Data reference
        private DragDropHandler dragDropHandler;
        private InventorySlot slot;
        private int slotNumber; // 0-9

        // State
        private bool isHovered = false;
        private bool isSelected = false;
        private Vector3 originalScale;

        // Events
        public event Action<HotbarSlotUI> OnSlotClicked;
        public event Action<HotbarSlotUI> OnSlotRightClicked;
        public event Action<HotbarSlotUI> OnSlotHoverEnter;
        public event Action<HotbarSlotUI> OnSlotHoverExit;
        public event Action OnClick;

        // Properties
        public InventorySlot Slot => slot;
        public int SlotIndex => slot?.SlotIndex ?? -1;
        public int SlotNumber => slotNumber;
        public bool IsEmpty => slot?.IsEmpty ?? true;
        public bool IsHovered => isHovered;
        public bool IsSelected => isSelected;

        private void Awake()
        {
            originalScale = transform.localScale;
            ValidateReferences();

            dragDropHandler = GetComponent<DragDropHandler>();
            if (dragDropHandler == null)
                dragDropHandler = gameObject.AddComponent<DragDropHandler>();
        }

        private void ValidateReferences()
        {
            if (itemIcon == null)
                Debug.LogError("[HotBarSlotUI] ItemIcon not assigned!", this);

            if (quantityText == null)
                Debug.LogWarning("[HotBarSlotUI] QuantityText not assigned!", this);

            if (keyNumberText == null)
                Debug.LogWarning("[HotBarSlotUI] KeyNumberText not assigned!", this);

            if (backgroundImage == null)
                Debug.LogWarning("[HotBarSlotUI] BackgroundImage not assigned!", this);

            if (selectionBorder == null)
                Debug.LogWarning("[HotBarSlotUI] SelectionBorder not assigned!", this);
        }

        /// Bind slot data with UI
        public void Initialize(InventorySlot inventorySlot, int number)
        {
            if (inventorySlot == null)
            {
                Debug.LogError("[HotBarSlotUI] Cannot initialize with null slot!");
                return;
            }

            // Unsubscribe old slot if exists
            if (slot != null)
            {
                UnsubscribeFromSlot();
            }

            slot = inventorySlot;
            slotNumber = number;

            SubscribeToSlot();
            UpdateKeyNumber();
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

        private void UpdateKeyNumber()
        {
            if (keyNumberText == null) return;

            // Display 0-9 (0 represents the 10th slot)
            int displayNumber = slotNumber == 9 ? 0 : slotNumber + 1;
            keyNumberText.text = displayNumber.ToString();
        }

        // Update entire UI based on slot data
        public void UpdateUI()
        {
            if (slot == null)
            {
                // Clear UI if slot is null
                if (itemIcon != null)
                {
                    itemIcon.sprite = null;
                    itemIcon.enabled = false;
                }
                if (quantityText != null)
                {
                    quantityText.text = "";
                    quantityText.enabled = false;
                }
                return;
            }

            this.UpdateIcon();
            this.UpdateQuantity();
            this.UpdateBackground();
            this.UpdateSelectionBorder();
            this.UpdateLockedState();
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

            if (slot.IsLocked)
            {
                backgroundImage.color = lockedColor;
            }
            else if (isHovered && !isSelected)
            {
                backgroundImage.color = highlightColor;
            }
            else
            {
                backgroundImage.color = normalColor;
            }
        }

        private void UpdateSelectionBorder()
        {
            if (selectionBorder == null) return;

            selectionBorder.enabled = isSelected;
            if (isSelected)
            {
                selectionBorder.color = borderColor;
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
            UpdateSelectionBorder();
            UpdateBackground();
        }

        public void Deselect()
        {
            isSelected = false;
            UpdateSelectionBorder();
            UpdateBackground();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            UpdateBackground();
            OnSlotHoverEnter?.Invoke(this);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            UpdateBackground();
            OnSlotHoverExit?.Invoke(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (slot == null || slot.IsLocked) return;

            if (eventData.button == PointerEventData.InputButton.Left)
            {
                OnSlotClicked?.Invoke(this);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (slot == null || slot.IsEmpty || slot.IsLocked) return;

            GameplayBlocker.UIDragging = true; // 🔥 Chặn Farming/Tools
            dragDropHandler?.OnBeginDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            dragDropHandler?.OnDrag(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            dragDropHandler?.OnEndDrag(eventData);
            GameplayBlocker.UIDragging = false;
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (slot?.IsLocked ?? true) return;
            dragDropHandler?.OnDrop(eventData);
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

namespace TinyFarm
{
    public static class GameplayBlocker
    {
        public static bool UIDragging = false;
        public static bool UIOpened = false;
    }
}

