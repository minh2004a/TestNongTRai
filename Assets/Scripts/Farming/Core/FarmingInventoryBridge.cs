using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TinyFarm.Items;
using TinyFarm.Items.UI;

namespace TinyFarm.Farming
{
    // Bridge giữa FarmingController và InventoryManager
    // Handles: Remove seeds when planting, Add items when harvesting
    public class FarmingInventoryBridge : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FarmingController farmingController;
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private HotbarSystem hotbarSystem;
        
        [Header("Settings")]
        [SerializeField] private bool debugMode = true;
        
        private void Awake()
        {
            InitializeReferences();
        }
        
        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void InitializeReferences()
        {
            if (farmingController == null)
                farmingController = FindObjectOfType<FarmingController>();

            if (inventoryManager == null)
                inventoryManager = FindObjectOfType<InventoryManager>();

            if (hotbarSystem == null)
                hotbarSystem = FindObjectOfType<HotbarSystem>();
        }
        
        private void SubscribeToEvents()
        {
            if (farmingController != null)
            {
                farmingController.OnTileInteracted += HandleTileInteraction;
                LogDebug("Subscribed to FarmingController events");
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (farmingController != null)
            {
                farmingController.OnTileInteracted -= HandleTileInteraction;
            }
        }

        // ==========================================
        // EVENT HANDLERS
        // ==========================================

        private void HandleTileInteraction(Vector2Int gridPos)
        {
            // This is called AFTER the tile action completed
            // We don't need to do anything here for now
            // The actual inventory changes happen in the methods below
        }

        // ==========================================
        // PUBLIC API - Called by FarmingController
        // ==========================================

        /// <summary>
        /// Try to consume seed from inventory when planting
        /// Called BEFORE planting
        /// </summary>
        public bool TryConsumeSeed(Item heldSeed)
        {
            if (heldSeed == null || heldSeed.ItemData == null)
            {
                LogDebug("Cannot consume seed: held item is null");
                return false;
            }

            if (hotbarSystem == null)
            {
                Debug.LogWarning("[FarmingInventory] HotbarSystem not found!");
                return false;
            }

            // ✅ consume trực tiếp từ hotbar slot đang active
            int selectedSlot = hotbarSystem.SelectedSlotIndex;
            var slot = hotbarSystem.GetHotbarSlot(selectedSlot);

            if (slot == null || slot.IsEmpty)
            {
                LogDebug("Cannot consume seed: hotbar slot empty");
                return false;
            }

            Item removed = slot.RemoveItem(1);

            if (removed != null)
            {
                LogDebug($"Consumed 1x {heldSeed.ItemData.itemName} FROM HOTBAR");
                hotbarSystem?.RefreshHotbar();
                return true;
            }

            Debug.LogWarning("[FarmingInventory] Failed to remove seed from hotbar");
            return false;
        }

        /// Try consume 1 unit of the heldSeed (search hotbar first, then inventory)
        public bool TryConsumeSeedFromHeld(Item heldSeed)
        {
            if (heldSeed == null || heldSeed.ItemData == null)
            {
                LogDebug("Cannot consume seed: held item is null");
                return false;
            }

            string seedID = heldSeed.ItemData.itemID;

            // 1) Try hotbar (selected slot first, then all slots)
            if (hotbarSystem != null)
            {
                // try selected slot
                int sel = hotbarSystem.SelectedSlotIndex;
                InventorySlot selSlot = hotbarSystem.GetHotbarSlot(sel);
                if (selSlot != null && !selSlot.IsEmpty && selSlot.ItemID == seedID)
                {
                    Item removed = selSlot.RemoveItem(1);
                    if (removed != null)
                    {
                        LogDebug($"Consumed 1x {heldSeed.ItemData.itemName} FROM HOTBAR (selected slot)");
                        hotbarSystem.RefreshHotbar();
                        return true;
                    }
                }

                // try any hotbar slot (in case held item came from another hotbar slot)
                for (int i = 0; i < hotbarSystem.HotbarSize; i++)
                {
                    InventorySlot s = hotbarSystem.GetHotbarSlot(i);
                    if (s != null && !s.IsEmpty && s.ItemID == seedID)
                    {
                        Item removed = s.RemoveItem(1);
                        if (removed != null)
                        {
                            LogDebug($"Consumed 1x {heldSeed.ItemData.itemName} FROM HOTBAR (slot {i})");
                            hotbarSystem.RefreshHotbar();
                            return true;
                        }
                    }
                }
            }

            // 2) Fallback: try inventory manager
            if (inventoryManager != null)
            {
                bool removed = inventoryManager.RemoveItem(seedID, 1);
                if (removed)
                {
                    LogDebug($"Consumed 1x {heldSeed.ItemData.itemName} FROM INVENTORY");
                    return true;
                }
                else
                {
                    LogDebug($"Player doesn't have {heldSeed.ItemData.itemName} in inventory");
                }
            }
            else
            {
                Debug.LogWarning("[FarmingInventory] InventoryManager not found!");
            }

            return false;
        }

        /// <summary>
        /// Add harvested items to inventory
        /// Called AFTER harvesting
        /// </summary>
        public void AddHarvestedItems(List<Item> items)
        {
            if (items == null || items.Count == 0)
            {
                LogDebug("No items to add");
                return;
            }

            if (inventoryManager == null)
            {
                Debug.LogWarning("[FarmingInventory] InventoryManager not found!");
                return;
            }

            foreach (var item in items)
            {
                if (item?.ItemData != null)
                {
                    bool added = inventoryManager.AddItem(item.Clone());

                    if (added)
                    {
                        LogDebug($"Added {item.ItemData.itemName} to inventory");
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to add {item.ItemData.itemName} - inventory full?");
                        // TODO: Drop item on ground if inventory full
                    }
                }
            }
        }
        
        /// <summary>
        /// Check if player has seed in inventory
        /// </summary>
        public bool HasSeed(string seedID)
        {
            if (inventoryManager == null) return false;
            return inventoryManager.HasItem(seedID, 1);
        }

        /// <summary>
        /// Get seed count in inventory
        /// </summary>
        public int GetSeedCount(string seedID)
        {
            if (inventoryManager == null) return 0;
            return inventoryManager.GetItemCount(seedID);
        }

        public Item GetItemFromInventory(string itemID)
        {
            if (inventoryManager == null) return null;
            return inventoryManager.GetItemByID(itemID);
        }

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[FarmingInventory] {message}");
            }
        }
        
#if UNITY_EDITOR
        [ContextMenu("Debug - Log Inventory")]
        private void DebugLogInventory()
        {
            if (inventoryManager != null)
            {
                Debug.Log("=== INVENTORY ===");
                var slots = inventoryManager.GetAllSlots();
                foreach (var slot in slots)
                {
                    if (!slot.IsEmpty)
                    {
                        Debug.Log($"{slot.ItemName} x{slot.Quantity}");
                    }
                }
            }
        }
#endif
    }
}

