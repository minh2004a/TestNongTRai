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
        [SerializeField] private SpriteRenderer playerSpriteRenderer; // ⚠️ Assign manually từ Player GameObject

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
            Debug.Log("[ItemHolding] 🔧 Awake() called");
            InitializeComponents();
        }

        private void Start()
        {
            Debug.Log("[ItemHolding] ▶️ Start() called");
            ValidateSetup();
            SetupItemSprite();
            SubscribeToEvents();
            Debug.Log("[ItemHolding] ✅ Initialization complete");
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
            // ✅ Tự động tìm AnimController
            if (animController == null)
            {
                animController = GetComponent<PlayerAnimationController>();
                if (animController == null)
                    animController = GetComponentInParent<PlayerAnimationController>();
                if (animController == null)
                    animController = GetComponentInChildren<PlayerAnimationController>();
            }

            // ⚠️ KHÔNG tự động assign PlayerSpriteRenderer
            // Để user tự assign manually
        }

        private void InitializeComponents()
        {
            if (animController == null)
            {
                animController = GetComponent<PlayerAnimationController>();

                // ✅ Nếu không tìm thấy, tìm trong parent/children
                if (animController == null)
                {
                    animController = GetComponentInParent<PlayerAnimationController>();
                }
                if (animController == null)
                {
                    animController = GetComponentInChildren<PlayerAnimationController>();
                }

                Debug.Log($"[ItemHolding] AnimController: {(animController != null ? "✅ Found" : "❌ Missing")}");
            }

            // ⚠️ KHÔNG tự động GetComponent SpriteRenderer
            // Phải assign manually trong Inspector
            if (playerSpriteRenderer == null)
            {
                Debug.LogError("[ItemHolding] ❌ PlayerSpriteRenderer NOT assigned! Please assign manually in Inspector!");
            }
            else
            {
                Debug.Log($"[ItemHolding] PlayerSpriteRenderer: ✅ Assigned ({playerSpriteRenderer.gameObject.name})");
            }

            if (animController == null)
            {
                Debug.LogError("[ItemHolding] ❌ PlayerAnimationController not found!");
                enabled = false;
            }
        }

        private void ValidateSetup()
        {
            Debug.Log("[ItemHolding] 🔍 Validating setup...");

            if (animController == null)
            {
                Debug.LogError("[ItemHolding] ❌ Missing PlayerAnimationController!");
                enabled = false;
                return;
            }

            if (playerSpriteRenderer == null)
            {
                Debug.LogWarning("[ItemHolding] ⚠️ Missing player SpriteRenderer!");
            }

            Debug.Log($"[ItemHolding] AutoCreateItemSprite: {autoCreateItemSprite}");
        }

        private void SetupItemSprite()
        {
            Debug.Log("[ItemHolding] 🎨 Setting up item sprite...");

            // Tạo GameObject cho item sprite nếu chưa có
            if (itemSpriteRenderer == null && autoCreateItemSprite)
            {
                Debug.Log("[ItemHolding] 🆕 Creating ItemSprite GameObject...");

                GameObject itemObj = new GameObject("ItemSprite");
                itemObj.transform.SetParent(transform);
                itemObj.transform.localPosition = Vector3.zero;

                itemSpriteRenderer = itemObj.AddComponent<SpriteRenderer>();

                // Set sorting layer
                if (playerSpriteRenderer != null)
                {
                    itemSpriteRenderer.sortingLayerName = playerSpriteRenderer.sortingLayerName;
                    itemSpriteRenderer.sortingOrder = playerSpriteRenderer.sortingOrder + 1;
                    Debug.Log($"[ItemHolding] Sorting Layer: {itemSpriteRenderer.sortingLayerName}, Order: {itemSpriteRenderer.sortingOrder}");
                }
                else
                {
                    itemSpriteRenderer.sortingLayerName = "Player";
                    itemSpriteRenderer.sortingOrder = 10;
                    Debug.LogWarning("[ItemHolding] ⚠️ PlayerSpriteRenderer null, using default sorting");
                }

                itemHoldPoint = itemObj.transform;

                Debug.Log("[ItemHolding] ✅ ItemSprite created successfully");
            }
            else if (itemSpriteRenderer != null)
            {
                Debug.Log("[ItemHolding] ✅ ItemSpriteRenderer already assigned");
            }
            else
            {
                Debug.LogWarning("[ItemHolding] ⚠️ AutoCreate disabled and no ItemSpriteRenderer assigned!");
            }

            // Ẩn item sprite ban đầu
            if (itemSpriteRenderer != null)
            {
                itemSpriteRenderer.enabled = false;
                Debug.Log("[ItemHolding] ItemSprite hidden initially");
            }
        }

        private void SubscribeToEvents()
        {
            if (animController != null)
            {
                animController.OnStateChanged += OnAnimationStateChanged;
                Debug.Log("[ItemHolding] ✅ Subscribed to animation events");
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
            Debug.Log($"[ItemHolding] 📥 EquipItem() called with item: {(item != null ? item.ItemData?.itemName : "NULL")}");

            if (item == null)
            {
                Debug.LogWarning("[ItemHolding] ❌ Cannot equip null item");
                return false;
            }

            if (item.ItemData == null)
            {
                Debug.LogWarning("[ItemHolding] ❌ Item has null ItemData");
                return false;
            }

            Debug.Log($"[ItemHolding] Item details - Name: {item.ItemData.itemName}, ID: {item.ItemData.itemID}");

            // Check if item has sprite
            if (item.ItemData.icon == null)
            {
                Debug.LogWarning($"[ItemHolding] ⚠️ Item has no icon sprite: {item.ItemData.itemName}");
                return false;
            }

            Debug.Log($"[ItemHolding] Item icon: {item.ItemData.icon.name}");

            // Check if already equipped
            if (currentItem == item)
            {
                Debug.Log($"[ItemHolding] ℹ️ Item already equipped: {item.ItemData.itemName}");
                return true;
            }

            // Store old item
            Item oldItem = currentItem;

            // Equip new item
            currentItem = item;
            currentItemData = item.ItemData;
            isHoldingItem = true;

            Debug.Log($"[ItemHolding] ✅ Item equipped - isHoldingItem: {isHoldingItem}");

            // Update sprite
            UpdateItemSprite();

            // Show sprite
            if (itemSpriteRenderer != null)
            {
                itemSpriteRenderer.enabled = true;
                Debug.Log($"[ItemHolding] 👁️ ItemSprite enabled: {itemSpriteRenderer.enabled}");
            }
            else
            {
                Debug.LogError("[ItemHolding] ❌ ItemSpriteRenderer is NULL!");
            }

            // Fire events
            OnItemEquipped?.Invoke(item);

            if (oldItem != null)
            {
                OnItemChanged?.Invoke(oldItem, item);
            }

            Debug.Log($"[ItemHolding] ✅✅✅ Item successfully equipped: {item.ItemData.itemName}");

            return true;
        }

        /// <summary>
        /// Unequip item hiện tại
        /// </summary>
        public void UnequipItem()
        {
            Debug.Log($"[ItemHolding] 📤 UnequipItem() called - Current holding: {isHoldingItem}");

            if (!isHoldingItem)
            {
                Debug.Log("[ItemHolding] ℹ️ No item to unequip");
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
                Debug.Log("[ItemHolding] 👁️ ItemSprite hidden");
            }

            // Fire event
            OnItemUnequipped?.Invoke(oldItem);

            Debug.Log($"[ItemHolding] ✅ Unequipped item: {oldItem?.ItemData?.itemName}");
        }

        /// <summary>
        /// Quick equip từ ItemData (testing)
        /// </summary>
        public bool EquipItemData(ItemData itemData)
        {
            Debug.Log($"[ItemHolding] EquipItemData() called with: {(itemData != null ? itemData.itemName : "NULL")}");

            if (itemData == null)
            {
                Debug.LogWarning("[ItemHolding] ❌ Cannot equip null ItemData");
                return false;
            }

            // Create temporary item instance
            Item tempItem = new Item(itemData, 1);
            return EquipItem(tempItem);
        }

        // ==========================================
        // SPRITE UPDATE
        // ==========================================

        private void UpdateItemSprite()
        {
            Debug.Log("[ItemHolding] 🎨 UpdateItemSprite() called");

            if (itemSpriteRenderer == null)
            {
                Debug.LogError("[ItemHolding] ❌ ItemSpriteRenderer is NULL!");
                return;
            }

            if (currentItemData == null)
            {
                Debug.LogError("[ItemHolding] ❌ CurrentItemData is NULL!");
                return;
            }

            // Set sprite from ItemData
            itemSpriteRenderer.sprite = currentItemData.icon;
            itemSpriteRenderer.sortingLayerName = "Object";
            itemSpriteRenderer.sortingOrder = 10;
            itemSpriteRenderer.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
            Debug.Log($"[ItemHolding] Set sprite to: {(currentItemData.icon != null ? currentItemData.icon.name : "NULL")}");

            // Match player's flip
            if (playerSpriteRenderer != null)
            {
                itemSpriteRenderer.flipX = playerSpriteRenderer.flipX;
                Debug.Log($"[ItemHolding] FlipX: {itemSpriteRenderer.flipX}");
            }

            Debug.Log($"[ItemHolding] ✅ Item sprite updated: {currentItemData.itemName}");
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
            if (playerSpriteRenderer != null && playerSpriteRenderer.flipX && currentDirection == Direction.Side)
            {
                position.x = -position.x;
            }

            itemHoldPoint.localPosition = position;

            // Apply rotation
            itemHoldPoint.localEulerAngles = offset.rotation;

            // Update sorting order (in front or behind player)
            if (itemSpriteRenderer != null && playerSpriteRenderer != null)
            {
                int sortOrder = playerSpriteRenderer.sortingOrder + offset.sortingOffset;
                itemSpriteRenderer.sortingOrder = sortOrder;
            }

            // Update sprite flip
            if (itemSpriteRenderer != null && playerSpriteRenderer != null)
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

            if (currentItem != null && currentItemData != null)
            {
                Debug.Log($"Item: {currentItemData.itemName}");
                Debug.Log($"Item ID: {currentItemData.itemID}");
                Debug.Log($"Icon: {(currentItemData.icon != null ? currentItemData.icon.name : "NULL")}");
            }
            else
            {
                Debug.Log("Item: None");
            }

            if (itemSpriteRenderer != null)
            {
                Debug.Log($"ItemSprite GameObject: {itemSpriteRenderer.gameObject.name}");
                Debug.Log($"Sprite Enabled: {itemSpriteRenderer.enabled}");
                Debug.Log($"Current Sprite: {(itemSpriteRenderer.sprite != null ? itemSpriteRenderer.sprite.name : "NULL")}");
                Debug.Log($"Sorting Layer: {itemSpriteRenderer.sortingLayerName}");
                Debug.Log($"Sorting Order: {itemSpriteRenderer.sortingOrder}");
                Debug.Log($"Flip X: {itemSpriteRenderer.flipX}");
                Debug.Log($"Color: {itemSpriteRenderer.color}");
            }
            else
            {
                Debug.Log("ItemSpriteRenderer: NULL");
            }

            if (itemHoldPoint != null)
            {
                Debug.Log($"Hold Point Position: {itemHoldPoint.localPosition}");
                Debug.Log($"Hold Point Rotation: {itemHoldPoint.localEulerAngles}");
            }
            else
            {
                Debug.Log("ItemHoldPoint: NULL");
            }
        }

        [ContextMenu("Test - Unequip Item")]
        private void TestUnequipItem()
        {
            UnequipItem();
        }

        [ContextMenu("Test - Force Show ItemSprite")]
        private void TestForceShowSprite()
        {
            if (itemSpriteRenderer != null)
            {
                itemSpriteRenderer.enabled = true;
                Debug.Log("[ItemHolding] Force enabled ItemSprite");
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying || !isHoldingItem || itemHoldPoint == null)
                return;

            // Draw item position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(itemHoldPoint.position, 0.05f);

            // Draw line từ player đến item
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, itemHoldPoint.position);
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

