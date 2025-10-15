using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;

namespace TinyFarm.Items.UI
{
    public class DragDropHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private SlotUI slotUI;
        [SerializeField] private Canvas canvas;
        [SerializeField] private GraphicRaycaster raycaster;

        [Header("Drag Visual")]
        [SerializeField] private GameObject dragVisualPrefab;
        [SerializeField] private float dragAlpha = 0.6f;

        // State
        private GameObject dragVisual;
        private CanvasGroup dragCanvasGroup;
        private RectTransform dragRectTransform;
        private SlotUI sourceSlot;
        private Vector2 originalPosition;

        // Events
        public static event Action<SlotUI, SlotUI> OnItemDropped;
        public static event Action<SlotUI> OnDragStarted;
        public static event Action<SlotUI> OnDragEnded;

        private void Awake()
        {
            if (slotUI == null)
                slotUI = GetComponent<SlotUI>();

            if (canvas == null)
                canvas = GetComponentInParent<Canvas>();

            if (raycaster == null)
                raycaster = canvas?.GetComponent<GraphicRaycaster>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Check if can drag
            if (slotUI == null || slotUI.IsEmpty || slotUI.Slot.IsLocked)
            {
                eventData.pointerDrag = null;
                return;
            }

            sourceSlot = slotUI;
            originalPosition = transform.position;

            // Create drag visual
            CreateDragVisual();

            OnDragStarted?.Invoke(sourceSlot);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragVisual == null) return;

            // Move drag visual with cursor
            dragRectTransform.position = eventData.position;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragVisual == null) return;

            // Find target slot
            SlotUI targetSlot = GetSlotUnderCursor(eventData);

            if (targetSlot != null && targetSlot != sourceSlot)
            {
                // Try drop
                HandleDrop(sourceSlot, targetSlot);
            }

            // Cleanup
            DestroyDragVisual();

            OnDragEnded?.Invoke(sourceSlot);
            sourceSlot = null;
        }

        private void CreateDragVisual()
        {
            if (dragVisualPrefab != null)
            {
                dragVisual = Instantiate(dragVisualPrefab, canvas.transform);
            }
            else
            {
                // Create default visual
                dragVisual = new GameObject("DragVisual");
                dragVisual.transform.SetParent(canvas.transform);

                Image image = dragVisual.AddComponent<Image>();
                image.sprite = sourceSlot.Slot.ItemIcon;
                image.raycastTarget = false;
            }

            dragRectTransform = dragVisual.GetComponent<RectTransform>();
            dragRectTransform.sizeDelta = new Vector2(64, 64);

            // Add CanvasGroup for alpha
            dragCanvasGroup = dragVisual.GetComponent<CanvasGroup>();
            if (dragCanvasGroup == null)
            {
                dragCanvasGroup = dragVisual.AddComponent<CanvasGroup>();
            }
            dragCanvasGroup.alpha = dragAlpha;
            dragCanvasGroup.blocksRaycasts = false;
        }

        private void DestroyDragVisual()
        {
            if (dragVisual != null)
            {
                Destroy(dragVisual);
                dragVisual = null;
            }
        }

        private SlotUI GetSlotUnderCursor(PointerEventData eventData)
        {
            List<RaycastResult> results = new List<RaycastResult>();
            raycaster.Raycast(eventData, results);

            foreach (var result in results)
            {
                SlotUI slot = result.gameObject.GetComponent<SlotUI>();
                if (slot != null && slot != sourceSlot)
                {
                    return slot;
                }
            }

            return null;
        }

        private void HandleDrop(SlotUI source, SlotUI target)
        {
            if (source == null || target == null) return;
            if (target.Slot.IsLocked) return;

            // Check if can merge
            bool canMerge = !source.IsEmpty && !target.IsEmpty &&
                           source.Slot.CanMergeWith(target.Slot);

            if (canMerge)
            {
                // Merge stacks
                target.Slot.MergeWith(source.Slot);
            }
            else
            {
                // Swap slots
                source.Slot.SwapWith(target.Slot);
            }

            OnItemDropped?.Invoke(source, target);
        }
    }
}

