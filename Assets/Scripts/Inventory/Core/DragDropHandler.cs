using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;

namespace TinyFarm.Items.UI
{
    public class DragDropHandler : MonoBehaviour, 
        IBeginDragHandler, 
        IDragHandler, 
        IEndDragHandler, 
        IDropHandler
    {
        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private GraphicRaycaster raycaster;

        [Header("Drag Visual Settings")]
        [SerializeField] private float dragAlpha = 0.7f;
        [SerializeField] private Vector2 dragIconSize = new Vector2(64, 64);
        [SerializeField] private Color validDropColor = new Color(0, 1, 0, 0.3f);
        [SerializeField] private Color invalidDropColor = new Color(1, 0, 0, 0.3f);

        // Drag state - Static để share giữa tất cả instances
        private static GameObject dragVisual;
        private static Image dragIcon;
        private static CanvasGroup dragCanvasGroup;
        private static RectTransform dragRectTransform;
        private static ISlotUIBase draggedSlot;
        private static ISlotUIBase hoveredSlot;

        // Highlight
        private Image highlightImage;
        private GameObject highlightOverlay;

        // Component references
        private ISlotUIBase slotUIBase;

        private void Awake()
        {
            // Try get SlotUI first, then HotBarSlotUI
            slotUIBase = GetComponent<SlotUI>();
            if (slotUIBase == null)
            {
                slotUIBase = GetComponent<HotbarSlotUI>();
            }

            if (slotUIBase == null)
            {
                Debug.LogError("[DragDropHandler] No valid slot UI component found (SlotUI or HotBarSlotUI)!", this);
            }

            if (canvas == null)
            {
                canvas = GetComponentInParent<Canvas>();
            }

            if (raycaster == null)
            {
                raycaster = canvas?.GetComponent<GraphicRaycaster>();
            }

            CreateHighlightOverlay();
        }

        private void CreateHighlightOverlay()
        {
            // Create highlight overlay for this slot
            highlightOverlay = new GameObject("DropHighlight");
            highlightOverlay.transform.SetParent(transform, false);

            RectTransform rect = highlightOverlay.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;

            highlightImage = highlightOverlay.AddComponent<Image>();
            highlightImage.color = validDropColor;
            highlightImage.raycastTarget = false;

            highlightOverlay.SetActive(false);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Validate can drag
            if (slotUIBase == null || slotUIBase.IsEmpty || slotUIBase.Slot.IsLocked)
            {
                eventData.pointerDrag = null;
                return;
            }

            draggedSlot = slotUIBase;
            CreateDragVisual();

            Debug.Log($"[DragDrop] Started dragging: {draggedSlot.Slot.ItemID} from {slotUIBase.GetType().Name}");
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragVisual == null || draggedSlot == null) return;

            // Move drag visual
            dragRectTransform.position = eventData.position;

            // Find slot under cursor
            ISlotUIBase targetSlot = GetSlotUnderCursor(eventData);
            UpdateHoveredSlot(targetSlot);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (draggedSlot == null) return;

            // Find target slot
            ISlotUIBase targetSlot = GetSlotUnderCursor(eventData);

            if (targetSlot != null && targetSlot != draggedSlot)
            {
                HandleDrop(draggedSlot, targetSlot);
            }

            // Cleanup
            CleanupDrag();
        }

        public void OnDrop(PointerEventData eventData)
        {
            // This is called on the DROP TARGET
            if (draggedSlot == null || draggedSlot == slotUIBase) return;

            HandleDrop(draggedSlot, slotUIBase);
        }

        private void CreateDragVisual()
        {
            if (dragVisual != null)
            {
                Destroy(dragVisual);
            }

            // Create drag visual
            dragVisual = new GameObject("DragIcon");
            dragVisual.transform.SetParent(canvas.transform, false);

            // Setup RectTransform
            dragRectTransform = dragVisual.AddComponent<RectTransform>();
            dragRectTransform.sizeDelta = dragIconSize;
            dragRectTransform.pivot = new Vector2(0.5f, 0.5f);

            // Add Image
            dragIcon = dragVisual.AddComponent<Image>();
            dragIcon.sprite = draggedSlot.Slot.ItemIcon;
            dragIcon.raycastTarget = false;
            dragIcon.preserveAspect = true;

            // Add CanvasGroup for alpha
            dragCanvasGroup = dragVisual.AddComponent<CanvasGroup>();
            dragCanvasGroup.alpha = dragAlpha;
            dragCanvasGroup.blocksRaycasts = false;
            dragCanvasGroup.interactable = false;

            // Set high sort order
            Canvas dragCanvas = dragVisual.AddComponent<Canvas>();
            dragCanvas.overrideSorting = true;
            dragCanvas.sortingOrder = 1000;
        }

        private void CleanupDrag()
        {
            if (dragVisual != null)
            {
                Destroy(dragVisual);
                dragVisual = null;
            }

            if (hoveredSlot != null)
            {
                ClearHighlight(hoveredSlot);
                hoveredSlot = null;
            }

            draggedSlot = null;
        }

        private void UpdateHoveredSlot(ISlotUIBase newHoveredSlot)
        {
            if (hoveredSlot == newHoveredSlot) return;

            // Clear old highlight
            if (hoveredSlot != null)
            {
                ClearHighlight(hoveredSlot);
            }

            // Set new highlight
            hoveredSlot = newHoveredSlot;

            if (hoveredSlot != null && hoveredSlot != draggedSlot)
            {
                bool canDrop = CanDropOnSlot(draggedSlot, hoveredSlot);
                ShowHighlight(hoveredSlot, canDrop);
            }
        }

        private void ShowHighlight(ISlotUIBase slot, bool isValid)
        {
            var handler = (slot as MonoBehaviour)?.GetComponent<DragDropHandler>();
            if (handler != null && handler.highlightOverlay != null)
            {
                handler.highlightOverlay.SetActive(true);
                handler.highlightImage.color = isValid ? validDropColor : invalidDropColor;
            }
        }

        private void ClearHighlight(ISlotUIBase slot)
        {
            var handler = (slot as MonoBehaviour)?.GetComponent<DragDropHandler>();
            if (handler != null && handler.highlightOverlay != null)
            {
                handler.highlightOverlay.SetActive(false);
            }
        }

        private ISlotUIBase GetSlotUnderCursor(PointerEventData eventData)
        {
            if (raycaster == null) return null;

            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(eventData, results);

            foreach (var result in results)
            {
                // Check for SlotUI
                SlotUI slotUI = result.gameObject.GetComponent<SlotUI>();
                if (slotUI != null)
                    return slotUI;

                slotUI = result.gameObject.GetComponentInParent<SlotUI>();
                if (slotUI != null)
                    return slotUI;

                // Check for HotBarSlotUI
                HotbarSlotUI hotBarSlotUI = result.gameObject.GetComponent<HotbarSlotUI>();
                if (hotBarSlotUI != null)
                    return hotBarSlotUI;

                hotBarSlotUI = result.gameObject.GetComponentInParent<HotbarSlotUI>();
                if (hotBarSlotUI != null)
                    return hotBarSlotUI;
            }

            return null;
        }

        private bool CanDropOnSlot(ISlotUIBase source, ISlotUIBase target)
        {
            if (source == null || target == null) return false;
            if (source == target) return false;
            if (target.Slot.IsLocked) return false;

            // Can always drop on empty slot
            if (target.IsEmpty) return true;

            // Check if can merge
            if (source.Slot.CanMergeWith(target.Slot))
            {
                return !target.Slot.IsFull;
            }

            // Can swap if both have items
            return true;
        }

        private void HandleDrop(ISlotUIBase source, ISlotUIBase target)
        {
            if (source == null || target == null)
            {
                Debug.LogWarning("[DragDrop] Invalid drop: source or target is null");
                return;
            }

            if (!CanDropOnSlot(source, target))
            {
                Debug.LogWarning("[DragDrop] Cannot drop on this slot");
                return;
            }

            // Check if can merge
            bool canMerge = !target.IsEmpty && source.Slot.CanMergeWith(target.Slot);

            if (canMerge)
            {
                // Merge stacks
                bool merged = target.Slot.MergeWith(source.Slot);
                if (merged)
                {
                    Debug.Log($"[DragDrop] Merged {source.Slot.ItemID} into slot {target.SlotIndex}");
                }
                else
                {
                    Debug.LogWarning("[DragDrop] Merge failed");
                }
            }
            else
            {
                // Swap slots
                source.Slot.SwapWith(target.Slot);
                Debug.Log($"[DragDrop] Swapped slot {source.SlotIndex} ↔ {target.SlotIndex}");
            }

            // Force refresh both slots
            source.UpdateUI();
            target.UpdateUI();
        }

        private void OnDestroy()
        {
            // Cleanup if this was the dragged slot
            if (draggedSlot == slotUIBase)
            {
                CleanupDrag();
            }
        }

        // Public method to force cleanup (useful for debugging)
        public static void ForceCleanup()
        {
            if (dragVisual != null)
            {
                Destroy(dragVisual);
                dragVisual = null;
            }
            draggedSlot = null;
            hoveredSlot = null;
        }
    }
}

