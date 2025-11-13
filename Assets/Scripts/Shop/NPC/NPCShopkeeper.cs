using System.Collections;
using System.Collections.Generic;
using TinyFarm.Farming;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.NPC
{
    // NPC Shopkeeper - Click chuột phải để mở shop
    // Bán seeds theo mùa, mua crops từ player
    public class NPCShopkeeper : MonoBehaviour
    {
        [Header("NPC Info")]
        [SerializeField] private string npcName = "Pierre";
        [SerializeField] private Sprite npcPortrait;

        [Header("Shop Settings")]
        [SerializeField] private ShopInventoryData shopInventory;
        [SerializeField] private float interactionRange = 2f;
        
        [Header("Price Modifiers")]
        [SerializeField, Range(0.5f, 2f)] private float buyPriceMultiplier = 1.0f;    // Player mua từ shop
        [SerializeField, Range(0.3f, 1f)] private float sellPriceMultiplier = 1.0f;  // Player bán cho shop

        [Header("Seasonal Items")]
        [SerializeField] private bool sellSeedsBySeasonOnly = true;

        [Header("Debug")]
        [SerializeField] private bool debugMode = true;
        
        // Runtime
        private bool isPlayerNearby = false;
        private Transform playerTransform;
        
        // Properties
        public string NPCName => npcName;
        public Sprite NPCPortrait => npcPortrait;
        public ShopInventoryData ShopInventory => shopInventory;
        public float BuyPriceMultiplier => buyPriceMultiplier;
        public float SellPriceMultiplier => sellPriceMultiplier;
        
        // Events
        public event System.Action<NPCShopkeeper> OnShopOpened;
        public event System.Action OnShopClosed;

        private void Start()
        {
            FindPlayer();
        }

        private void Update()
        {
            CheckPlayerDistance();
            HandleInput();
        }

        private void FindPlayer()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        private void CheckPlayerDistance()
        {
            if (playerTransform == null) return;

            float distance = Vector3.Distance(transform.position, playerTransform.position);
            bool wasNearby = isPlayerNearby;
            isPlayerNearby = distance <= interactionRange;

            if (isPlayerNearby && !wasNearby)
            {
                LogDebug($"Player nearby {npcName}");
            }
            else if (!isPlayerNearby && wasNearby)
            {
                LogDebug($"Player left {npcName}");
            }
        }

        private void HandleInput()
        {
            if (!isPlayerNearby) return;

            if (Input.GetMouseButtonDown(1))
            {
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0;

                Collider2D hit = Physics2D.OverlapPoint(mouseWorldPos);
                if (hit != null && hit.gameObject == gameObject)
                {
                    OpenShop();
                }
            }
        }

        public void OpenShop()
        {
            if (shopInventory == null) return;

            OnShopOpened?.Invoke(this);
        }

        public void CloseShop()
        {
            OnShopClosed?.Invoke();
        }

        // SHOP INVENTORY

        // Get items available in shop this season
        public List<ShopItemEntry> GetAvailableItems()
        {
            if (shopInventory == null) return new List<ShopItemEntry>();

            Season currentSeason = TimeManager.Instance?.GetCurrentSeason() ?? Season.Spring;

            return shopInventory.GetItemsForSeason(currentSeason, sellSeedsBySeasonOnly);
        }

        // Calculate buy price (player buying from shop)
        public int GetBuyPrice(ItemData itemData)
        {
            if (itemData == null) return 0;

            int basePrice = itemData.buyPrice;
            return Mathf.RoundToInt(basePrice * buyPriceMultiplier);
        }

        // Calculate sell price (player selling to shop)
        public int GetSellPrice(ItemData itemData)
        {
            if (itemData == null) return 0;

            int basePrice = itemData.sellPrice;
            return Mathf.RoundToInt(basePrice * sellPriceMultiplier);
        }

        // Check if shop accepts this item
        public bool CanBuyFromPlayer(ItemData itemData)
        {
            if (itemData == null) return false;

            if (itemData.GetItemType() == ItemType.Crop)
                return true;

            return false;
        }
        
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[NPCShop] {message}");
            }
        }
    }
}

