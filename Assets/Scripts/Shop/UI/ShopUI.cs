using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TinyFarm.Items;
using TinyFarm.Items.UI;


namespace TinyFarm.NPC
{
    // ShopUI - Match với UI structure của bạn
    // PanelNPC: Portrait + Message
    // ShopItemsList: ScrollList với SlotShop items
    // InventoryGrid: SlotsContainer với inventory slots
    public class ShopUI : MonoBehaviour
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject shopPanel;
        
        [Header("NPC Panel")]
        [SerializeField] private Image npcPortraitIcon;      // PanelNPC/ImgNPC/Icon
        [SerializeField] private TextMeshProUGUI npcMessageText; // PanelNPC/NPCMessage/Text
        
        [Header("Shop Items List")]
        [SerializeField] private Transform shopItemsContainer;  // ShopItemsList/ScrollList/SlotsContainer
        [SerializeField] private GameObject shopItemSlotPrefab; // SlotShop prefab
        
        [Header("Inventory Grid")]
        [SerializeField] private Transform inventoryContainer;  // InventoryGrid/SlotsContainer
        [SerializeField] private GameObject inventorySlotPrefab; // Slot prefab cho inventory
        
        [Header("Gold Display")]
        [SerializeField] private Image goldIcon;              // MoneyIcon/Icon_Gold
        [SerializeField] private TextMeshProUGUI goldText;    // MoneyIcon/MoneyText
        
        [Header("Scrollbar")]
        [SerializeField] private Scrollbar shopScrollbar;     // ShopItemsList/Scrollbar
        
        [Header("References")]
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private PlayerWallet playerWallet;
        
        // Runtime
        private NPCShopkeeper currentShopkeeper;
        private List<ShopItemSlotUI> shopSlots = new List<ShopItemSlotUI>();
        private List<InventorySlotUI> inventorySlots = new List<InventorySlotUI>();
        private ShopItemEntry selectedItem;

        private void Awake()
        {
            InitializeReferences();
            Hide();
        }
        private void Start()
        {
            SubscribeToShopkeepers();
        }
        
        private void OnDestroy()
        {
            UnsubscribeFromShopkeepers();
        }
        
        private void InitializeReferences()
        {
            if (inventoryManager == null)
                inventoryManager = FindObjectOfType<InventoryManager>();
                
            if (playerWallet == null)
                playerWallet = FindObjectOfType<PlayerWallet>();
                
            // Subscribe to gold changes
            if (playerWallet != null)
            {
                playerWallet.OnGoldChanged += UpdateGoldDisplay;
            }
        }

        // SHOPKEEPER EVENTS

        private void SubscribeToShopkeepers()
        {
            var shopkeepers = FindObjectsOfType<NPCShopkeeper>();
            foreach (var shop in shopkeepers)
            {
                shop.OnShopOpened += OnShopOpened;
            }
        }
        
        private void UnsubscribeFromShopkeepers()
        {
            var shopkeepers = FindObjectsOfType<NPCShopkeeper>();
            foreach (var shop in shopkeepers)
            {
                if (shop != null)
                    shop.OnShopOpened -= OnShopOpened;
            }
            
            if (playerWallet != null)
            {
                playerWallet.OnGoldChanged -= UpdateGoldDisplay;
            }
        }
        
        private void OnShopOpened(NPCShopkeeper shopkeeper)
        {
            currentShopkeeper = shopkeeper;
            Show();
        }

        // SHOW/HIDE

        public void Show()
        {
            if (currentShopkeeper == null)
            {
                Debug.LogWarning("[ShopUI] No shopkeeper assigned!");
                return;
            }
            
            shopPanel.SetActive(true);
            
            // Update NPC info
            UpdateNPCPanel();
            
            // Populate shop items
            PopulateShopItems();
            
            // Populate player inventory
            PopulateInventoryGrid();
            
            // Update gold display
            UpdateGoldDisplay(playerWallet?.CurrentGold ?? 0);
            
            // Block gameplay
            TinyFarm.GameplayBlocker.UIOpened = true;
            
            Debug.Log($"[ShopUI] Opened {currentShopkeeper.NPCName}'s shop");
        }

        public void Hide()
        {
            shopPanel.SetActive(false);
            
            ClearUI();
            
            currentShopkeeper?.CloseShop();
            currentShopkeeper = null;
            
            // Unblock gameplay
            TinyFarm.GameplayBlocker.UIOpened = false;
            
            Debug.Log("[ShopUI] Shop closed");
        }

        // NPC PANEL

        private void UpdateNPCPanel()
        {
            if (currentShopkeeper == null) return;
            
            // Update portrait
            if (npcPortraitIcon != null && currentShopkeeper.NPCPortrait != null)
            {
                npcPortraitIcon.sprite = currentShopkeeper.NPCPortrait;
            }
            
            // Update message
            if (npcMessageText != null)
            {
                npcMessageText.text = $"Chào mừng đến tiệm {currentShopkeeper.NPCName}! Chú chỉ nhập những mặt hàng tốt nhất.";
            }
        }

        // SHOP ITEMS LIST

        private void PopulateShopItems()
        {
            // Clear existing slots
            ClearShopSlots();
            
            if (currentShopkeeper == null || shopItemsContainer == null)
            {
                Debug.LogWarning("[ShopUI] Cannot populate shop items - missing references");
                return;
            }
            
            // Get available items for current season
            var availableItems = currentShopkeeper.GetAvailableItems();
            
            Debug.Log($"[ShopUI] Populating {availableItems.Count} shop items");
            
            // Create shop item slots
            foreach (var entry in availableItems)
            {
                if (entry.itemData == null) continue;
                
                // Instantiate slot
                GameObject slotObj = Instantiate(shopItemSlotPrefab, shopItemsContainer);
                ShopItemSlotUI slotUI = slotObj.GetComponent<ShopItemSlotUI>();
                
                if (slotUI != null)
                {
                    // Setup slot
                    int buyPrice = currentShopkeeper.GetBuyPrice(entry.itemData);
                    slotUI.Setup(entry, buyPrice);
                    
                    // Subscribe to click event
                    slotUI.OnClicked += () => OnShopItemClicked(entry);
                    
                    shopSlots.Add(slotUI);
                }
                else
                {
                    Debug.LogWarning("[ShopUI] ShopItemSlotUI component not found on prefab!");
                    Destroy(slotObj);
                }
            }
            
            // Reset scrollbar
            if (shopScrollbar != null)
            {
                shopScrollbar.value = 1f; // Top
            }
        }

        private void ClearShopSlots()
        {
            foreach (var slot in shopSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            shopSlots.Clear();
        }

        // INVENTORY GRID

        private void PopulateInventoryGrid()
        {
            // Clear existing slots
            ClearInventorySlots();
            
            if (inventoryManager == null || inventoryContainer == null)
            {
                Debug.LogWarning("[ShopUI] Cannot populate inventory - missing references");
                return;
            }
            
            // Get all inventory slots
            var allSlots = inventoryManager.GetAllSlots();
            
            Debug.Log($"[ShopUI] Populating {allSlots.Count} inventory slots");
            
            // Create inventory slot UIs
            foreach (var invSlot in allSlots)
            {
                GameObject slotObj = Instantiate(inventorySlotPrefab, inventoryContainer);
                InventorySlotUI slotUI = slotObj.GetComponent<InventorySlotUI>();
                
                if (slotUI != null)
                {
                    slotUI.SetSlot(invSlot);
                    slotUI.OnSlotClicked += (clickedSlot) => OnInventorySlotClicked(clickedSlot);
                    
                    inventorySlots.Add(slotUI);
                }
                else
                {
                    Destroy(slotObj);
                }
            }
        }
        
        private void ClearInventorySlots()
        {
            foreach (var slot in inventorySlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            inventorySlots.Clear();
        }

        // ITEM SELECTION & TRANSACTION

        private void OnShopItemClicked(ShopItemEntry entry)
        {
            selectedItem = entry;
            
            Debug.Log($"[ShopUI] Selected shop item: {entry.itemData.itemName}");
            
            // Try buy item
            TryBuyItem(entry);
        }
        
        private void OnInventorySlotClicked(InventorySlot invSlot)
        {
            if (invSlot.IsEmpty) return;
            
            Debug.Log($"[ShopUI] Selected inventory item: {invSlot.ItemName}");
            
            // Try sell item
            TrySellItem(invSlot.Item);
        }

        private void TryBuyItem(ShopItemEntry entry)
        {
            if (entry == null || currentShopkeeper == null)
            {
                Debug.LogWarning("[ShopUI] Cannot buy - missing entry or shopkeeper");
                return;
            }
            
            ItemData itemData = entry.itemData;
            int price = currentShopkeeper.GetBuyPrice(itemData);
            
            // Check if player has enough gold
            if (!playerWallet.HasGold(price))
            {
                Debug.Log($"[ShopUI] Not enough gold! Need {price}g, have {playerWallet.CurrentGold}g");
                // TODO: Show "Not enough gold" message
                return;
            }
            
            // Check if shop has stock
            if (!entry.HasStock())
            {
                Debug.Log("[ShopUI] Item out of stock!");
                return;
            }
            
            // Try add to inventory
            Item newItem = new Item(itemData, 1);
            bool added = inventoryManager.AddItem(newItem, 1);
            
            if (added)
            {
                // Deduct gold
                playerWallet.RemoveGold(price);
                
                // Deduct stock
                entry.TryPurchase(1);
                
                // Refresh UI
                PopulateInventoryGrid();
                PopulateShopItems(); // Refresh to show updated stock
                
                Debug.Log($"[ShopUI] Bought {itemData.itemName} for {price}g");
            }
            else
            {
                Debug.Log("[ShopUI] Inventory full!");
                // TODO: Show "Inventory full" message
            }
        }

        private void TrySellItem(Item item)
        {
            if (item == null || currentShopkeeper == null) return;
            
            ItemData itemData = item.ItemData;
            
            // Check if shop accepts this item
            if (!currentShopkeeper.CanBuyFromPlayer(itemData))
            {
                Debug.Log($"[ShopUI] Shop doesn't buy {itemData.itemName}");
                return;
            }
            
            int sellPrice = currentShopkeeper.GetSellPrice(itemData);
            
            // Remove from inventory
            bool removed = inventoryManager.RemoveItem(itemData.itemID, 1);
            
            if (removed)
            {
                // Give gold
                playerWallet.AddGold(sellPrice);
                
                // Refresh inventory
                PopulateInventoryGrid();
                
                Debug.Log($"[ShopUI] Sold {itemData.itemName} for {sellPrice}g");
            }
        }

        // GOLD DISPLAY

        private void UpdateGoldDisplay(int amount)
        {
            if (goldText != null)
            {
                goldText.text = amount.ToString();
            }
        }

        // UTILITIES

        private void ClearUI()
        {
            ClearShopSlots();
            ClearInventorySlots();
            selectedItem = null;
        }
        
        // PUBLIC API (for Close Button)
        public void OnCloseButtonClicked()
        {
            Hide();
        }
    }
}

