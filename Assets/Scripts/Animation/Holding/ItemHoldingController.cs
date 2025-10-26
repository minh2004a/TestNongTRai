using System;
using TinyFarm.Animation;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.PlayerInput
{
    // Quản lý visual của item đang cầm trên tay
    // Show/hide item sprite, position theo animation
    public class ItemHoldingController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerAnimationController animController;
        [SerializeField] private SpriteRenderer heldItemRenderer;
        [SerializeField] private Transform itemHoldPoint;

        [Header("Settings")]
        [SerializeField] private Vector2 idleOffset = new Vector2(0.3f, -0.2f);
        [SerializeField] private Vector2 runOffset = new Vector2(0.3f, -0.1f);
        [SerializeField] private float itemScale = 1f;

        [Header("Runtime State")]
        [SerializeField] private Item currentHeldItem;
        [SerializeField] private bool isHoldingItem = false;

        // Events
        public event Action<Item> OnItemHeld;
        public event Action OnItemReleased;

        // Properties
        public Item CurrentHeldItem => currentHeldItem;
        public bool IsHoldingItem => isHoldingItem;

        // ==========================================
        // INITIALIZATION
        // ==========================================

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            SetupItemRenderer();
        }

        private void OnValidate()
        {
            if (animController == null)
                animController = GetComponent<PlayerAnimationController>();
        }

        private void InitializeComponents()
        {
            if (animController == null)
                animController = GetComponent<PlayerAnimationController>();

            // Create item renderer if not assigned
            if (heldItemRenderer == null)
            {
                CreateItemRenderer();
            }

            // Create hold point if not assigned
            if (itemHoldPoint == null)
            {
                CreateItemHoldPoint();
            }
        }

        private void CreateItemRenderer()
        {
            GameObject rendererObj = new GameObject("HeldItemRenderer");
            rendererObj.transform.SetParent(transform);
            rendererObj.transform.localPosition = Vector3.zero;

            heldItemRenderer = rendererObj.AddComponent<SpriteRenderer>();
            heldItemRenderer.sortingLayerName = "Player"; // Adjust as needed
            heldItemRenderer.sortingOrder = 10; // Above player
            heldItemRenderer.enabled = false;
        }

        private void CreateItemHoldPoint()
        {
            GameObject holdPointObj = new GameObject("ItemHoldPoint");
            holdPointObj.transform.SetParent(transform);
            holdPointObj.transform.localPosition = Vector3.zero;
            itemHoldPoint = holdPointObj.transform;
        }

        private void SetupItemRenderer()
        {
            if (heldItemRenderer != null)
            {
                heldItemRenderer.enabled = false;
            }
        }

        // ==========================================
        // PUBLIC API - HOLD/RELEASE
        // ==========================================

        /// <summary>
        /// Hold item (show sprite on hand)
        /// </summary>
        public void HoldItem(Item item)
        {
            if (item == null)
            {
                ReleaseItem();
                return;
            }

            currentHeldItem = item;
            isHoldingItem = true;

            // Set sprite
            if (heldItemRenderer != null && item.ItemData != null)
            {
                heldItemRenderer.sprite = item.ItemData.icon;
                heldItemRenderer.enabled = true;

                // Apply scale
                heldItemRenderer.transform.localScale = Vector3.one * itemScale;
            }

            // Fire event
            OnItemHeld?.Invoke(item);

            Debug.Log($"[ItemHolding] Holding: {item.Name}");
        }

        /// <summary>
        /// Release item (hide sprite)
        /// </summary>
        public void ReleaseItem()
        {
            currentHeldItem = null;
            isHoldingItem = false;

            if (heldItemRenderer != null)
            {
                heldItemRenderer.enabled = false;
                heldItemRenderer.sprite = null;
            }

            // Fire event
            OnItemReleased?.Invoke();

            Debug.Log("[ItemHolding] Released item");
        }

        // ==========================================
        // UPDATE - POSITION ITEM
        // ==========================================

        private void LateUpdate()
        {
            if (!isHoldingItem || heldItemRenderer == null)
                return;

            UpdateItemPosition();
            UpdateItemFlip();
        }

        private void UpdateItemPosition()
        {
            // Position based on animation state
            Vector2 offset = animController.IsMoving ? runOffset : idleOffset;

            // Adjust based on direction
            if (animController.CurrentDirection == Direction.Side)
            {
                // Horizontal
                offset.x = animController.IsFacingLeft ? -Mathf.Abs(offset.x) : Mathf.Abs(offset.x);
            }

            // Apply position
            if (itemHoldPoint != null)
            {
                itemHoldPoint.localPosition = offset;
                heldItemRenderer.transform.position = itemHoldPoint.position;
            }
            else
            {
                heldItemRenderer.transform.localPosition = offset;
            }
        }

        private void UpdateItemFlip()
        {
            // Flip item sprite based on facing direction
            if (animController.CurrentDirection == Direction.Side)
            {
                heldItemRenderer.flipX = animController.IsFacingLeft;
            }
            else
            {
                heldItemRenderer.flipX = false;
            }
        }

        // ==========================================
        // UTILITY
        // ==========================================

        /// Check if holding specific item
        //public bool IsHoldingItem(string itemID)
        //{
        //    return isHoldingItem && currentHeldItem?.ItemData?.itemID == itemID;
        //}

        /// <summary>
        /// Set item scale
        /// </summary>
        public void SetItemScale(float scale)
        {
            itemScale = scale;

            if (heldItemRenderer != null)
            {
                heldItemRenderer.transform.localScale = Vector3.one * scale;
            }
        }
    }
}

