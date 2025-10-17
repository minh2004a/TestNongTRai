using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TinyFarm.Items;

namespace TinyFarm.Items.UI
{
    public class TrashCanUI : MonoBehaviour,
        IDropHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [Header("UI References")]
        public Image background;
        public Image trashIconClosed;
        public Image trashIconOpen;
        public GameObject warningOverlay;
        public CanvasGroup canvasGroup;

        [Header("Visual Settings")]
        public Color normalColor = Color.white;
        public Color hoverColor = new Color(1f, 0.8f, 0.8f); // Light red
        public Color dropColor = new Color(1f, 0.5f, 0.5f); // Red

        [Header("Animation")]
        public float animationSpeed = 5f;
        public float scaleOnHover = 1.1f;

        [Header("Confirmation")]
        public bool requireConfirmation = true;
        public string confirmationMessage = "Delete this item?";

        private bool isHoveringWithItem = false;
        private bool isOpen = false;
        private Vector3 originalScale;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            originalScale = rectTransform.localScale;

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }

            SetTrashState(false);

            if (warningOverlay != null)
                warningOverlay.SetActive(false);
        }

        private void Update()
        {
            // Check if user is dragging something
            bool isDragging = Input.GetMouseButton(0) &&
                              EventSystem.current.currentSelectedGameObject != null;

            // Animate trash can open/close
            if (isHoveringWithItem && !isOpen)
            {
                SetTrashState(true);
            }
            else if (!isHoveringWithItem && isOpen)
            {
                SetTrashState(false);
            }

            // Smooth scale animation
            Vector3 targetScale = isHoveringWithItem ?
                originalScale * scaleOnHover : originalScale;

            rectTransform.localScale = Vector3.Lerp(
                rectTransform.localScale,
                targetScale,
                Time.deltaTime * animationSpeed
            );
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            // Check if dragging an item
            if (eventData.pointerDrag != null)
            {
                SlotUI slotUI = eventData.pointerDrag.GetComponent<SlotUI>();

                if (slotUI != null && slotUI.GetInventorySlot() != null &&
                    !slotUI.GetInventorySlot().IsEmpty)
                {
                    isHoveringWithItem = true;

                    if (background != null)
                        background.color = hoverColor;

                    if (warningOverlay != null)
                        warningOverlay.SetActive(true);
                }
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHoveringWithItem = false;

            if (background != null)
                background.color = normalColor;

            if (warningOverlay != null)
                warningOverlay.SetActive(false);
        }

        public void OnDrop(PointerEventData eventData)
        {
            GameObject droppedObject = eventData.pointerDrag;

            if (droppedObject == null) return;

            // Get SlotUI
            SlotUI slotUI = droppedObject.GetComponent<SlotUI>();

            if (slotUI != null)
            {
                InventorySlot slot = slotUI.GetInventorySlot();

                if (slot != null && !slot.IsEmpty)
                {
                    DeleteItem(slot, slotUI);
                }
            }

            // Reset state
            isHoveringWithItem = false;

            if (background != null)
                background.color = normalColor;

            if (warningOverlay != null)
                warningOverlay.SetActive(false);
        }

        private void DeleteItem(InventorySlot slot, SlotUI slotUI)
        {
            string itemName = slot.Item.Name;
            int quantity = slot.Quantity;

            // Show confirmation dialog (optional)
            if (requireConfirmation)
            {
                // TODO: Implement confirmation dialog
                // For now, just log and delete
                Debug.LogWarning($"[TrashCan] Deleting {quantity}x {itemName}");
            }

            // Play delete animation
            PlayDeleteAnimation();

            // Delete item
            InventoryManager.Instance.ClearSlot(slot.SlotIndex);

            // Update UI
            slotUI.UpdateUI();

            Debug.Log($"[TrashCan] 🗑️ Deleted {quantity}x {itemName}");

            // Play sound effect
            // AudioManager.Instance.PlaySFX("trash_delete");
        }

        private void SetTrashState(bool open)
        {
            isOpen = open;

            if (trashIconClosed != null)
                trashIconClosed.enabled = !open;

            if (trashIconOpen != null)
                trashIconOpen.enabled = open;
        }

        private void PlayDeleteAnimation()
        {
            // Flash red
            if (background != null)
            {
                background.color = dropColor;
                StartCoroutine(FadeBackToNormal());
            }

            // Scale animation
            StartCoroutine(ScalePulse());
        }

        private System.Collections.IEnumerator FadeBackToNormal()
        {
            float elapsed = 0f;
            float duration = 0.3f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                if (background != null)
                {
                    background.color = Color.Lerp(
                        dropColor,
                        normalColor,
                        elapsed / duration
                    );
                }

                yield return null;
            }
        }

        private System.Collections.IEnumerator ScalePulse()
        {
            float elapsed = 0f;
            float duration = 0.2f;
            Vector3 pulseScale = originalScale * 1.2f;

            // Scale up
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                rectTransform.localScale = Vector3.Lerp(
                    originalScale,
                    pulseScale,
                    elapsed / duration
                );
                yield return null;
            }

            elapsed = 0f;

            // Scale back down
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                rectTransform.localScale = Vector3.Lerp(
                    pulseScale,
                    originalScale,
                    elapsed / duration
                );
                yield return null;
            }

            rectTransform.localScale = originalScale;
        }

        // Show/Hide trash can
        public void SetVisible(bool visible)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.blocksRaycasts = visible;
            }
        }

        [ContextMenu("Test Delete Animation")]
        private void TestAnimation()
        {
            PlayDeleteAnimation();
        }
    }
}

