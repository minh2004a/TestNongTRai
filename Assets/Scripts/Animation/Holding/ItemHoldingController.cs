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
        [SerializeField] private SpriteRenderer playerSpriteRenderer;

        [Header("Item Sprite")]
        [SerializeField] private SpriteRenderer itemSpriteRenderer;
        [SerializeField] private Transform itemHoldPoint;

        [Header("Settings")]
        [SerializeField] private bool debugMode = true;
        [SerializeField] private bool autoCreateItemSprite = true;

        [Header("Position Offsets (per Direction)")]
        [SerializeField]
        private ItemHoldOffset offsetDown = new ItemHoldOffset(
            new Vector3(0, 1f, 0),
            new Vector3(0, 0, 0f),
            1
        );
        [SerializeField]
        private ItemHoldOffset offsetUp = new ItemHoldOffset(
            new Vector3(0, 1f, 0),
            new Vector3(0, 0, 0f),
            -1
        );
        [SerializeField]
        private ItemHoldOffset offsetSide = new ItemHoldOffset(
            new Vector3(0f, 1f, 0),
            new Vector3(0, 0, 0f),
            1
        );

        [Header("Runtime Info")]
        [SerializeField] private Item currentItem;
        [SerializeField] private ItemData currentItemData;
        [SerializeField] private bool isHoldingItem = false;
        [SerializeField] private Direction currentDirection = Direction.Down;

        // Properties
        public bool IsHoldingItem => isHoldingItem;
        public Item CurrentItem => currentItem;
        public ItemData CurrentItemData => currentItemData;

        // Events
        public event Action<Item> OnItemEquipped;
        public event Action<Item> OnItemUnequipped;
        public event Action<Item, Item> OnItemChanged;

        // ==========================================
        // INITIALIZATION
        // ==========================================

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            ValidateSetup();
            SetupItemSprite();
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void LateUpdate()
        {
            // Update item position/rotation theo animation
            if (isHoldingItem)
            {
                UpdateItemTransform();
            }
        }

        private void OnValidate()
        {
            if (animController == null)
                animController = GetComponent<PlayerAnimationController>();

            if (playerSpriteRenderer == null)
                playerSpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void InitializeComponents()
        {
            if (animController == null)
                animController = GetComponent<PlayerAnimationController>();

            if (playerSpriteRenderer == null)
                playerSpriteRenderer = GetComponent<SpriteRenderer>();

            if (animController == null)
            {
                Debug.LogError("[ItemHolding] PlayerAnimationController not found!");
                enabled = false;
            }
        }

        private void ValidateSetup()
        {
            if (animController == null)
            {
                Debug.LogError("[ItemHolding] Missing PlayerAnimationController!");
                enabled = false;
            }

            if (playerSpriteRenderer == null)
            {
                Debug.LogWarning("[ItemHolding] Missing player SpriteRenderer!");
            }
        }

        private void SetupItemSprite()
        {
            // Tạo GameObject cho item sprite nếu chưa có
            if (itemSpriteRenderer == null && autoCreateItemSprite)
            {
                GameObject itemObj = new GameObject("ItemSprite");
                itemObj.transform.SetParent(transform);
                itemObj.transform.localPosition = Vector3.zero;

                itemSpriteRenderer = itemObj.AddComponent<SpriteRenderer>();
                itemSpriteRenderer.sortingLayerName = "Player"; // Same as player
                itemSpriteRenderer.sortingOrder = playerSpriteRenderer.sortingOrder + 1; // Above player

                itemHoldPoint = itemObj.transform;

                LogDebug("Created item sprite renderer automatically");
            }

            // Ẩn item sprite ban đầu
            if (itemSpriteRenderer != null)
            {
                itemSpriteRenderer.enabled = false;
            }
        }

        private void SubscribeToEvents()
        {
            if (animController != null)
            {
                animController.OnStateChanged += OnAnimationStateChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (animController != null)
            {
                animController.OnStateChanged -= OnAnimationStateChanged;
            }
        }

        // ==========================================
        // PUBLIC API - EQUIP/UNEQUIP
        // ==========================================

        /// <summary>
        /// Equip item để hiển thị trên tay (Seeds, consumables, etc.)
        /// </summary>
        public bool EquipItem(Item item)
        {
            if (item == null || item.ItemData == null)
            {
                Debug.LogWarning("[ItemHolding] Cannot equip null item");
                return false;
            }

            // Check if item has sprite
            if (item.ItemData.icon == null)
            {
                Debug.LogWarning($"[ItemHolding] Item has no sprite: {item.ItemData.itemName}");
                return false;
            }

            // Check if already equipped
            if (currentItem == item)
            {
                LogDebug($"Item already equipped: {item.ItemData.itemName}");
                return true;
            }

            // Store old item
            Item oldItem = currentItem;

            // Equip new item
            currentItem = item;
            currentItemData = item.ItemData;
            isHoldingItem = true;

            // Update sprite
            UpdateItemSprite();

            // Show sprite
            if (itemSpriteRenderer != null)
            {
                itemSpriteRenderer.enabled = true;
            }

            // Fire events
            OnItemEquipped?.Invoke(item);

            if (oldItem != null)
            {
                OnItemChanged?.Invoke(oldItem, item);
            }

            LogDebug($"Equipped item: {item.ItemData.itemName}");

            return true;
        }

        /// <summary>
        /// Unequip item hiện tại
        /// </summary>
        public void UnequipItem()
        {
            if (!isHoldingItem)
            {
                LogDebug("No item to unequip");
                return;
            }

            Item oldItem = currentItem;

            // Clear item
            currentItem = null;
            currentItemData = null;
            isHoldingItem = false;

            // Hide sprite
            if (itemSpriteRenderer != null)
            {
                itemSpriteRenderer.enabled = false;
                itemSpriteRenderer.sprite = null;
            }

            // Fire event
            OnItemUnequipped?.Invoke(oldItem);

            LogDebug($"Unequipped item: {oldItem?.ItemData?.itemName}");
        }

        // Quick equip từ ItemData (testing)
        public bool EquipItemData(ItemData itemData)
        {
            if (itemData == null)
                return false;

            // Create temporary item instance
            Item tempItem = new Item(itemData, 1);
            return EquipItem(tempItem);
        }

        // ==========================================
        // SPRITE UPDATE
        // ==========================================

        private void UpdateItemSprite()
        {
            if (itemSpriteRenderer == null || currentItemData == null)
                return;

            itemSpriteRenderer.sprite = currentItemData.icon;
            itemSpriteRenderer.sortingLayerName = "Object";
            itemSpriteRenderer.sortingOrder = 10;
        }

        private void UpdateItemTransform()
        {
            if (itemHoldPoint == null || !isHoldingItem)
                return;

            // Get current direction
            currentDirection = animController.CurrentDirection;

            // Get offset based on direction
            ItemHoldOffset offset = GetOffsetForDirection(currentDirection);

            // Apply position
            Vector3 position = offset.position;

            // Flip X position nếu facing left
            if (playerSpriteRenderer.flipX && currentDirection == Direction.Side)
            {
                position.x = -position.x;
            }

            itemHoldPoint.localPosition = position;

            // Apply rotation
            itemHoldPoint.localEulerAngles = offset.rotation;

            // Update sorting order (in front or behind player)
            if (itemSpriteRenderer != null)
            {
                int sortOrder = playerSpriteRenderer.sortingOrder + offset.sortingOffset;
                itemSpriteRenderer.sortingOrder = sortOrder;
            }

            // Update sprite flip
            if (itemSpriteRenderer != null)
            {
                itemSpriteRenderer.flipX = playerSpriteRenderer.flipX;
            }
        }

        private ItemHoldOffset GetOffsetForDirection(Direction direction)
        {
            switch (direction)
            {
                case Direction.Up:
                    return offsetUp;
                case Direction.Down:
                    return offsetDown;
                case Direction.Side:
                    return offsetSide;
                default:
                    return offsetDown;
            }
        }

        // ==========================================
        // EVENT HANDLERS
        // ==========================================

        private void OnAnimationStateChanged(AnimationState newState)
        {
            // Update item transform khi animation state thay đổi
            if (isHoldingItem)
            {
                UpdateItemTransform();
            }
        }

        // ==========================================
        // PUBLIC API - GETTERS
        // ==========================================

        public Item GetCurrentItem()
        {
            return currentItem;
        }

        public ItemData GetCurrentItemData()
        {
            return currentItemData;
        }

        public bool IsItemEquipped()
        {
            return isHoldingItem;
        }

        public bool IsItemEquipped(string itemID)
        {
            return isHoldingItem && currentItemData != null && currentItemData.itemID == itemID;
        }

        // ==========================================
        // PUBLIC API - CONFIGURATION
        // ==========================================

        /// <summary>
        /// Set offset cho direction cụ thể
        /// </summary>
        public void SetOffset(Direction direction, Vector3 position, Vector3 rotation, int sortingOffset)
        {
            ItemHoldOffset offset = new ItemHoldOffset(position, rotation, sortingOffset);

            switch (direction)
            {
                case Direction.Up:
                    offsetUp = offset;
                    break;
                case Direction.Down:
                    offsetDown = offset;
                    break;
                case Direction.Side:
                    offsetSide = offset;
                    break;
            }

            LogDebug($"Updated offset for {direction}");
        }

        /// <summary>
        /// Update item sprite color (ví dụ: dim khi out of uses)
        /// </summary>
        public void SetItemColor(Color color)
        {
            if (itemSpriteRenderer != null)
            {
                itemSpriteRenderer.color = color;
            }
        }

        /// <summary>
        /// Reset item color về white
        /// </summary>
        public void ResetItemColor()
        {
            SetItemColor(Color.white);
        }

        // ==========================================
        // DEBUG
        // ==========================================

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[ItemHolding] {message}");
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Debug - Log Current State")]
        private void DebugLogState()
        {
            Debug.Log("=== ITEM HOLDING STATE ===");
            Debug.Log($"Is Holding Item: {isHoldingItem}");
            Debug.Log($"Current Direction: {currentDirection}");

            if (currentItem != null)
            {
                Debug.Log($"Item: {currentItemData.itemName}");
                Debug.Log($"Item ID: {currentItemData.itemID}");
            }
            else
            {
                Debug.Log("Item: None");
            }

            if (itemHoldPoint != null)
            {
                Debug.Log($"Hold Point Position: {itemHoldPoint.localPosition}");
                Debug.Log($"Hold Point Rotation: {itemHoldPoint.localEulerAngles}");
            }

            if (itemSpriteRenderer != null)
            {
                Debug.Log($"Sprite Enabled: {itemSpriteRenderer.enabled}");
                Debug.Log($"Sorting Order: {itemSpriteRenderer.sortingOrder}");
                Debug.Log($"Flip X: {itemSpriteRenderer.flipX}");
            }
        }

        [ContextMenu("Test - Unequip Item")]
        private void TestUnequipItem()
        {
            UnequipItem();
        }
        
#endif
    }

    // ==========================================
    // HELPER CLASSES
    // ==========================================

    /// <summary>
    /// Offset configuration cho mỗi direction
    /// </summary>
    [System.Serializable]
    public struct ItemHoldOffset
    {
        public Vector3 position;
        public Vector3 rotation;
        public int sortingOffset; // +1 = in front, -1 = behind

        public ItemHoldOffset(Vector3 pos, Vector3 rot, int sort)
        {
            position = pos;
            rotation = rot;
            sortingOffset = sort;
        }
    }
}

