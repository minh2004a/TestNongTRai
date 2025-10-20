using System;
using TinyFarm.Items;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TinyFarm.Items.UI
{
    public class TrashCanUI : MonoBehaviour,
        IDropHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("Visual References")]
        [SerializeField] private Image trashCanImage;
        [SerializeField] private Sprite closedSprite;
        [SerializeField] private Sprite openSprite;

        [Header("Settings")]
        [SerializeField] private bool requireConfirmation = true;
        [SerializeField] private float autoCloseDelay = 0.5f;

        [Header("Visual Feedback")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private bool enableScaleAnimation = true;
        [SerializeField] private float hoverScale = 1.1f;
        [SerializeField] private float animationSpeed = 10f;

        // State
        private bool isHovered = false;
        private bool isOpen = false;
        private Vector3 originalScale;
        private float closeTimer = 0.5f;

        // Events
        public event Action<Item> OnItemTrashed;
        public event Action OnTrashCanOpened;
        public event Action OnTrashCanClosed;

        private void Awake()
        {
            ValidateReferences();
            originalScale = transform.localScale;
            Close();
        }

        private void ValidateReferences()
        {
            if (trashCanImage == null)
            {
                trashCanImage = GetComponent<Image>();
                if (trashCanImage == null)
                {
                    Debug.LogError("[TrashCanUI] TrashCanImage not found!", this);
                }
            }

            if (closedSprite == null)
                Debug.LogWarning("[TrashCanUI] ClosedSprite not assigned!", this);

            if (openSprite == null)
                Debug.LogWarning("[TrashCanUI] OpenSprite not assigned!", this);
        }

        private void Update()
        {
            // Auto close after delay
            if (isOpen && !isHovered)
            {
                closeTimer += Time.deltaTime;
                if (closeTimer >= autoCloseDelay)
                {
                    Close();
                }
            }

            // Scale animation
            if (enableScaleAnimation)
            {
                Vector3 targetScale = isHovered ? originalScale * hoverScale : originalScale;
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * animationSpeed);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovered = true;
            Open();
            UpdateVisuals();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovered = false;
            closeTimer = 0.5f;
            UpdateVisuals();
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("[TrashCanUI] OnDrop called");

            // Get the dragged object
            GameObject draggedObject = eventData.pointerDrag;
            if (draggedObject == null)
            {
                Debug.LogWarning("[TrashCanUI] No dragged object!");
                return;
            }

            // Try to get slot UI from dragged object
            SlotUI slotUI = draggedObject.GetComponent<SlotUI>();
            HotbarSlotUI hotbarSlotUI = draggedObject.GetComponent<HotbarSlotUI>();

            InventorySlot slot = null;

            if (slotUI != null)
            {
                slot = slotUI.Slot;
            }
            else if (hotbarSlotUI != null)
            {
                slot = hotbarSlotUI.Slot;
            }

            if (slot == null || slot.IsEmpty)
            {
                Debug.LogWarning("[TrashCanUI] Invalid slot or empty slot!");
                return;
            }

            // Confirmation
            if (requireConfirmation)
            {
                if (ConfirmTrash(slot))
                {
                    TrashItem(slot);
                }
            }
            else
            {
                TrashItem(slot);
            }
        }

        private bool ConfirmTrash(InventorySlot slot)
        {
            // Simple confirmation - can be replaced with a proper UI dialog
            string itemName = slot.ItemName;
            int quantity = slot.Quantity;

            string message = quantity > 1
                ? $"Delete {quantity}x {itemName}?"
                : $"Delete {itemName}?";

            Debug.Log($"[TrashCanUI] {message}");

            // For now, always return true
            // TODO: Implement proper confirmation dialog
            return true;
        }

        private void TrashItem(InventorySlot slot)
        {
            if (slot == null || slot.IsEmpty)
                return;

            Item trashedItem = slot.Item;
            string itemName = slot.ItemName;
            int quantity = slot.Quantity;

            Debug.Log($"[TrashCanUI] Trashing {quantity}x {itemName}");

            // Clear the slot
            slot.Clear();

            // Trigger event
            OnItemTrashed?.Invoke(trashedItem);

            // Visual feedback
            PlayTrashEffect();
        }

        private void PlayTrashEffect()
        {
            // Simple shake effect or particle effect can be added here
            // For now, just open the lid briefly
            Open();
            closeTimer = 0f;

            Debug.Log("[TrashCanUI] Item trashed!");
        }

        private void Open()
        {
            if (isOpen) return;

            isOpen = true;
            closeTimer = 0f;

            if (openSprite != null && trashCanImage != null)
            {
                trashCanImage.sprite = openSprite;
            }

            OnTrashCanOpened?.Invoke();
            Debug.Log("[TrashCanUI] Trash can opened");
        }

        private void Close()
        {
            if (!isOpen) return;

            isOpen = false;

            if (closedSprite != null && trashCanImage != null)
            {
                trashCanImage.sprite = closedSprite;
            }

            OnTrashCanClosed?.Invoke();
        }

        private void UpdateVisuals()
        {
            if (trashCanImage == null) return;
        }

        // Public methods
        public void SetConfirmationRequired(bool required)
        {
            requireConfirmation = required;
        }

        public void SetAutoCloseDelay(float delay)
        {
            autoCloseDelay = Mathf.Max(0f, delay);
        }

        public void ForceOpen()
        {
            Open();
        }

        public void ForceClose()
        {
            Close();
        }
    }
}

