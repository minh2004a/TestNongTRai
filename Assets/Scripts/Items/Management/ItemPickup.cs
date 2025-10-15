using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    [RequireComponent(typeof(Collider2D))]
    public class ItemPickup : MonoBehaviour
    {
        [Header("Item Data")]
        [Tooltip("Item instance này đang hold")]
        [SerializeField] private Item item;

        [Tooltip("Hoặc tạo item từ ID")]
        [SerializeField] private string itemID;

        [Tooltip("Số lượng (nếu tạo từ ID)")]
        [SerializeField] private int quantity = 1;

        [Header("Visual")]
        [Tooltip("SpriteRenderer để hiển thị icon")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Tooltip("Text hiển thị số lượng (optional)")]
        [SerializeField] private TMPro.TextMeshPro quantityText;

        [Header("Pickup Settings")]
        [Tooltip("Có thể nhặt được không?")]
        [SerializeField] private bool canPickup = true;

        [Tooltip("Auto pickup khi player chạm vào?")]
        [SerializeField] private bool autoPickup = true;

        [Tooltip("Pickup radius (nếu không dùng auto)")]
        [SerializeField] private float pickupRadius = 1.5f;

        [Tooltip("Delay trước khi có thể nhặt (giây)")]
        [SerializeField] private float pickupDelay = 0.5f;

        [Header("Animation")]
        [Tooltip("Bounce animation khi spawn")]
        [SerializeField] private bool enableBounce = true;

        [SerializeField] private float bounceHeight = 0.5f;
        [SerializeField] private float bounceDuration = 0.5f;

        [Header("Lifetime")]
        [Tooltip("Tự hủy sau X giây (0 = không tự hủy)")]
        [SerializeField] private float lifetime = 60f;

        [Tooltip("Blink trước khi hủy")]
        [SerializeField] private bool blinkBeforeDestroy = true;
        [SerializeField] private float blinkStartTime = 5f;

        // State
        private bool isPickedUp = false;
        private float spawnTime;
        private Collider2D pickupCollider;
        private Vector3 originalPosition;

        // Events
        public event Action<ItemPickup, GameObject> OnItemPickedUp;
        public event Action<ItemPickup> OnItemDestroyed;

        // Properties
        public Item Item => item;
        public bool CanPickup => canPickup && !isPickedUp && Time.time >= spawnTime + pickupDelay;
        public bool IsPickedUp => isPickedUp;

        private void Awake()
        {
            pickupCollider = GetComponent<Collider2D>();
            pickupCollider.isTrigger = true;
            spriteRenderer = GetComponent<SpriteRenderer>();

            spawnTime = Time.time;
            originalPosition = transform.position;
        }

        private void Start()
        {
            InitializeItem();
            UpdateVisuals();

            if (enableBounce)
            {
                StartCoroutine(BounceAnimation());
            }

            if (lifetime > 0)
            {
                StartCoroutine(LifetimeCoroutine());
            }
        }

        // Initialize item từ ID hoặc sử dụng item có sẵn
        private void InitializeItem()
        {
            if (item == null && !string.IsNullOrEmpty(itemID))
            {
                item = ItemManager.Instance.CreateItem(itemID, quantity);
            }

            if (item == null)
            {
                Debug.LogError("ItemPickup has no item!", this);
                Destroy(gameObject);
            }
        }

        // Update sprite và text hiển thị
        /// </summary>
        private void UpdateVisuals()
        {
            if (item == null) return;

            // Update sprite
            if (spriteRenderer != null && item.Icon != null)
            {
                spriteRenderer.sprite = item.Icon;
            }

            // Update quantity text
            if (quantityText != null)
            {
                if (item.IsStackable && item.CurrentStack > 1)
                {
                    quantityText.text = item.CurrentStack.ToString();
                    quantityText.gameObject.SetActive(true);
                }
                else
                {
                    quantityText.gameObject.SetActive(false);
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!autoPickup) return;

            TryPickup(other.gameObject);
        }

        // Thử nhặt item
        public bool TryPickup(GameObject picker)
        {
            if (!CanPickup) return false;

            // Kiểm tra picker có InventoryManager không
            var inventory = picker.GetComponent<InventoryManager>();
            if (inventory == null)
            {
                Debug.LogWarning($"{picker.name} has no InventoryManager!");
                return false;
            }

            // Thử add item vào inventory
            if (inventory.AddItem(item.ItemData.ItemID, quantity))
            {
                OnPickupSuccess(picker);
                return true;
            }
            else
            {
                Debug.Log("Inventory full!");
                return false;
            }
        }

        // Pickup thành công
        private void OnPickupSuccess(GameObject picker)
        {
            isPickedUp = true;

            OnItemPickedUp?.Invoke(this, picker);

            // Play sound effect (nếu có)
            // AudioManager.Instance?.PlaySFX("item_pickup");

            // Destroy GameObject
            Destroy(gameObject);
        }

        // Manual pickup (không dùng trigger)
        // Gọi từ player input
        public bool ManualPickup(GameObject picker)
        {
            if (!CanPickup) return false;

            // Check distance
            float distance = Vector3.Distance(transform.position, picker.transform.position);
            if (distance > pickupRadius)
            {
                return false;
            }

            return TryPickup(picker);
        }

        private IEnumerator BounceAnimation()
        {
            float elapsed = 0f;
            Vector3 startPos = transform.position;
            Vector3 targetPos = startPos + Vector3.up * bounceHeight;

            while (elapsed < bounceDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / bounceDuration;

                // Parabolic bounce
                float height = Mathf.Sin(t * Mathf.PI) * bounceHeight;
                transform.position = startPos + Vector3.up * height;

                yield return null;
            }

            transform.position = startPos;
        }

        private IEnumerator LifetimeCoroutine()
        {
            yield return new WaitForSeconds(lifetime - blinkStartTime);

            // Blink animation
            if (blinkBeforeDestroy)
            {
                float blinkInterval = 0.2f;
                float blinkTime = 0f;

                while (blinkTime < blinkStartTime)
                {
                    spriteRenderer.enabled = !spriteRenderer.enabled;
                    yield return new WaitForSeconds(blinkInterval);
                    blinkTime += blinkInterval;
                }
            }

            // Destroy
            OnItemDestroyed?.Invoke(this);
            Destroy(gameObject);
        }

        // Set item cho pickup này
        public void SetItem(Item newItem)
        {
            this.item = newItem;
            UpdateVisuals();
        }

        private void OnDrawGizmosSelected()
        {
            // Draw pickup radius
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, pickupRadius);
        }

        private void OnDestroy()
        {
            if (!isPickedUp && item != null)
            {
                // Item không được nhặt, destroy item instance
                item.Destroy();
            }
        }
    }
}

