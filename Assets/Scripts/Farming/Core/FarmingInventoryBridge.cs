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
        [SerializeField] private bool autoAddToInventory = true;
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
        public bool TryConsumeSeed(Item seedItem)
        {
            if (seedItem == null || seedItem.ItemData == null)
            {
                LogDebug("Cannot consume seed: item is null");
                return false;
            }

            if (inventoryManager == null)
            {
                Debug.LogWarning("[FarmingInventory] InventoryManager not found!");
                return false;
            }

            // Check if player has the seed
            if (!inventoryManager.HasItem(seedItem.ItemData.itemID, 1))
            {
                LogDebug($"Player doesn't have {seedItem.ItemData.itemName}");
                return false;
            }

            // Remove 1 seed from inventory
            bool removed = inventoryManager.RemoveItem(seedItem.ItemData.itemID, 1);

            if (removed)
            {
                LogDebug($"Consumed 1x {seedItem.ItemData.itemName}");
                return true;
            }
            else
            {
                Debug.LogWarning($"Failed to remove {seedItem.ItemData.itemName} from inventory");
                return false;
            }
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

