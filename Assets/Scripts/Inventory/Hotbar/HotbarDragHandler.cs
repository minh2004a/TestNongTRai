//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.EventSystems;
//using System.Collections.Generic;

//namespace TinyFarm.Items.UI
//{
//    public class HotbarDragHandler : MonoBehaviour
//    {
//        private static GameObject dragVisual;
//        private static Image dragIcon;
//        private static CanvasGroup dragCanvasGroup;
//        private static RectTransform dragRectTransform;
//        private static Canvas canvas;

//        private static HotBarSlotUI draggedSlot;
//        private static HotBarSlotUI hoveredSlot;
//        private static SlotUI inventoryDraggedSlot; // Track inventory drags

//        // Settings
//        private static readonly Color validDropColor = new Color(0, 1, 0, 0.3f);
//        private static readonly Color invalidDropColor = new Color(1, 0, 0, 0.3f);
//        private static readonly float dragAlpha = 0.7f;

//        public static void BeginDrag(HotBarSlotUI slot, PointerEventData eventData)
//        {
//            if (slot.IsEmpty) return;

//            draggedSlot = slot;
//            CreateDragVisual(slot);

//            Debug.Log($"[HotbarDrag] Started dragging from hotbar slot {slot.SlotIndex}");
//        }

//        public static void Drag(PointerEventData eventData)
//        {
//            if (dragVisual == null) return;

//            dragRectTransform.position = eventData.position;
//        }

//        public static void EndDrag(PointerEventData eventData)
//        {
//            if (draggedSlot == null) return;

//            CleanupDrag();
//            draggedSlot = null;
//        }

//        public static void Drop(HotBarSlotUI targetSlot, PointerEventData eventData)
//        {
//            // Case 1: Drag từ hotbar slot khác
//            if (draggedSlot != null && draggedSlot != targetSlot)
//            {
//                SwapHotbarSlots(draggedSlot, targetSlot);
//                CleanupDrag();
//                draggedSlot = null;
//                return;
//            }

//            // Case 2: Drag từ inventory
//            if (inventoryDraggedSlot != null)
//            {
//                HandleInventoryToHotbarDrop(inventoryDraggedSlot, targetSlot);
//                return;
//            }
//        }

//        // ===== INVENTORY TO HOTBAR DRAG =====
//        public static void BeginInventoryDrag(SlotUI slot)
//        {
//            inventoryDraggedSlot = slot;
//            Debug.Log($"[HotbarDrag] Inventory drag started: {slot.SlotIndex}");
//        }

//        public static void EndInventoryDrag()
//        {
//            inventoryDraggedSlot = null;
//        }

//        private static void HandleInventoryToHotbarDrop(SlotUI inventorySlot, HotBarSlotUI hotbarSlot)
//        {
//            if (inventorySlot == null || inventorySlot.IsEmpty)
//            {
//                Debug.LogWarning("[HotbarDrag] Invalid inventory slot");
//                return;
//            }

//            // Copy item from inventory to hotbar
//            InventorySlot sourceSlot = inventorySlot.Slot;
//            InventorySlot targetSlot = hotbarSlot.BoundSlot;

//            if (targetSlot == null)
//            {
//                Debug.LogError("[HotbarDrag] Hotbar slot has no bound slot!");
//                return;
//            }

//            // Check if can merge
//            if (!targetSlot.IsEmpty && targetSlot.CanMergeWith(sourceSlot))
//            {
//                targetSlot.MergeWith(sourceSlot);
//                Debug.Log($"[HotbarDrag] Merged inventory item into hotbar slot {hotbarSlot.SlotIndex}");
//            }
//            else if (targetSlot.IsEmpty)
//            {
//                // Copy item to hotbar
//                Item itemCopy = sourceSlot.Item.Clone();
//                targetSlot.SetItem(itemCopy);
//                Debug.Log($"[HotbarDrag] Copied {itemCopy.Name} to hotbar slot {hotbarSlot.SlotIndex}");
//            }
//            else
//            {
//                // Swap
//                sourceSlot.SwapWith(targetSlot);
//                Debug.Log($"[HotbarDrag] Swapped inventory ↔ hotbar slot {hotbarSlot.SlotIndex}");
//            }

//            hotbarSlot.UpdateDisplay();
//            inventorySlot.UpdateUI();
//        }

//        // ===== SWAP HOTBAR SLOTS =====
//        private static void SwapHotbarSlots(HotBarSlotUI slot1, HotBarSlotUI slot2)
//        {
//            if (slot1.BoundSlot == null || slot2.BoundSlot == null)
//            {
//                Debug.LogError("[HotbarDrag] Cannot swap - bound slot is null!");
//                return;
//            }

//            slot1.BoundSlot.SwapWith(slot2.BoundSlot);

//            slot1.UpdateDisplay();
//            slot2.UpdateDisplay();

//            Debug.Log($"[HotbarDrag] Swapped hotbar slots {slot1.SlotIndex} ↔ {slot2.SlotIndex}");
//        }

//        // ===== VISUAL =====
//        private static void CreateDragVisual(HotBarSlotUI slot)
//        {
//            CleanupDrag();

//            if (canvas == null)
//            {
//                canvas = slot.GetComponentInParent<Canvas>();
//            }

//            dragVisual = new GameObject("HotbarDragIcon");
//            dragVisual.transform.SetParent(canvas.transform, false);

//            dragRectTransform = dragVisual.AddComponent<RectTransform>();
//            dragRectTransform.sizeDelta = new Vector2(50, 50);
//            dragRectTransform.pivot = new Vector2(0.5f, 0.5f);

//            dragIcon = dragVisual.AddComponent<Image>();
//            dragIcon.sprite = slot.BoundSlot.ItemIcon;
//            dragIcon.raycastTarget = false;

//            dragCanvasGroup = dragVisual.AddComponent<CanvasGroup>();
//            dragCanvasGroup.alpha = dragAlpha;
//            dragCanvasGroup.blocksRaycasts = false;

//            Canvas dragCanvas = dragVisual.AddComponent<Canvas>();
//            dragCanvas.overrideSorting = true;
//            dragCanvas.sortingOrder = 1000;
//        }

//        private static void CleanupDrag()
//        {
//            if (dragVisual != null)
//            {
//                Object.Destroy(dragVisual);
//                dragVisual = null;
//            }
//        }

//        public static void ForceCleanup()
//        {
//            CleanupDrag();
//            draggedSlot = null;
//            hoveredSlot = null;
//            inventoryDraggedSlot = null;
//        }
//    }
//}

