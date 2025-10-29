using System;
using TinyFarm.Animation;
using TinyFarm.Items;
using UnityEngine;
using TinyFarm.Tools;

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
            if (isHoldingItem)
            {
                UpdateItemTransform();
            }
        }

        private void OnValidate()
        {
            if (animController == null)
            {
                animController = GetComponent<PlayerAnimationController>();
                if (animController == null)
                    animController = GetComponentInParent<PlayerAnimationController>();
                if (animController == null)
                    animController = GetComponentInChildren<PlayerAnimationController>();
            }
        }

        private void InitializeComponents()
        {
            if (animController == null)
            {
                animController = GetComponent<PlayerAnimationController>();

                if (animController == null)
                {
                    animController = GetComponentInParent<PlayerAnimationController>();
                }
                if (animController == null)
                {
                    animController = GetComponentInChildren<PlayerAnimationController>();
                }
            }
        }

        private void ValidateSetup()
        {
            
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

                // Set sorting layer
                if (playerSpriteRenderer != null)
                {
                    itemSpriteRenderer.sortingLayerName = playerSpriteRenderer.sortingLayerName;
                    itemSpriteRenderer.sortingOrder = playerSpriteRenderer.sortingOrder + 1;
                }
                else
                {
                    itemSpriteRenderer.sortingLayerName = "Player";
                    itemSpriteRenderer.sortingOrder = 10;
                }

                itemHoldPoint = itemObj.transform;

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

        // Equip item để hiển thị trên tay (Seeds, consumables, etc.)
        public bool EquipItem(Item item)
        {
            var toolEquip = FindObjectOfType<ToolEquipmentController>();
            if (toolEquip != null && toolEquip.HasToolEquipped)
                toolEquip.UnequipTool();
            if (item == null || item.ItemData == null) return false;

            // If already equipped same item
            if (currentItem == item) return true;

            Item oldItem = currentItem;

            currentItem = item;
            currentItemData = item.ItemData;
            isHoldingItem = true;

            UpdateItemSprite();

            if (itemSpriteRenderer != null) itemSpriteRenderer.enabled = true;

            // IMPORTANT: notify anim controller to enter Carry mode
            if (animController != null)
            {
                animController.SetCarrying(true);
            }

            OnItemEquipped?.Invoke(item);
            if (oldItem != null) OnItemChanged?.Invoke(oldItem, item);

            return true;
        }

        // Unequip item hiện tại
        public void UnequipItem()
        {
            if (!isHoldingItem) return;

            Item old = currentItem;

            currentItem = null;
            currentItemData = null;
            isHoldingItem = false;

            if (itemSpriteRenderer != null)
            {
                itemSpriteRenderer.enabled = false;
                itemSpriteRenderer.sprite = null;
            }

            // IMPORTANT: notify anim controller to exit Carry mode
            if (animController != null)
            {
                animController.SetCarrying(false);
            }

            OnItemUnequipped?.Invoke(old);
        }

        // Quick equip từ ItemData (testing)
        public bool EquipItemData(ItemData itemData)
        {
            if (itemData == null)
            {
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

            if (itemSpriteRenderer == null) return;

            if (currentItemData == null) return;

            // Set sprite from ItemData
            itemSpriteRenderer.sprite = currentItemData.icon;
            itemSpriteRenderer.sortingLayerName = "Object";
            itemSpriteRenderer.sortingOrder = 10;
            itemSpriteRenderer.transform.localScale = new Vector3(1.5f, 1.5f, 1f);

            // Match player's flip
            if (playerSpriteRenderer != null)
            {
                itemSpriteRenderer.flipX = playerSpriteRenderer.flipX;
            }
        }

        private void UpdateItemTransform()
        {
            if (itemHoldPoint == null || !isHoldingItem) return;

            currentDirection = animController.CurrentDirection;
            ItemHoldOffset offset = GetOffsetForDirection(currentDirection);

            Vector3 position = offset.position;

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
                case Direction.Up: return offsetUp;
                case Direction.Down: return offsetDown;
                case Direction.Side: return offsetSide;
                default: return offsetDown;
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

        /// Set offset cho direction cụ thể
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
        }

        /// Update item sprite color (ví dụ: dim khi out of uses)
        public void SetItemColor(Color color)
        {
            if (itemSpriteRenderer != null)
            {
                itemSpriteRenderer.color = color;
            }
        }

        /// Reset item color về white
        public void ResetItemColor()
        {
            SetItemColor(Color.white);
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

