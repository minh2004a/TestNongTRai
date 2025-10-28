using System;
using UnityEngine;
using TinyFarm.Items;
using TinyFarm.Tools;
using TinyFarm.Animation;
using TinyFarm.PlayerInput;

namespace TinyFarm.Farming
{
    // Controller chính để player tương tác với farming system
    // Handle: Till, Water, Plant, Harvest, Fertilize
    public class FarmingController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FarmGrid farmGrid;
        [SerializeField] private FarmingInventoryBridge inventoryBridge;

        [SerializeField] private ToolEquipmentController toolEquipment;
        [SerializeField] private ItemHoldingController itemHolding;
        [SerializeField] private PlayerAnimationController animController;
        [SerializeField] private Camera mainCamera;
        
        [Header("Interaction Settings")]
        [SerializeField] private float interactionRange = 1.5f;
        [SerializeField] private bool debugMode = true;
        
        [Header("Runtime Info")]
        [SerializeField] private Vector2Int hoveredGridPos = Vector2Int.zero;
        [SerializeField] private Vector2Int selectedGridPos = Vector2Int.zero;
        [SerializeField] private bool hasHoveredTile = false;
        
        // Events
        public event Action<Vector2Int> OnTileHovered;
        public event Action<Vector2Int> OnTileSelected;
        public event Action<Vector2Int> OnTileInteracted;

         private void Awake()
        {
            InitializeReferences();
        }

        private void InitializeReferences()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;

            if (farmGrid == null)
                farmGrid = FarmGrid.Instance;

            if (toolEquipment == null)
                toolEquipment = GetComponent<ToolEquipmentController>();

            if (itemHolding == null)
                itemHolding = GetComponent<ItemHoldingController>();

            if (animController == null)
                animController = GetComponent<PlayerAnimationController>();
        }

        private void Update()
        {
            UpdateHoveredTile();
            HandleInput();
        }

        // ==========================================
        // TILE DETECTION
        // ==========================================

        private void UpdateHoveredTile()
        {
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;

            FarmTile tile = farmGrid?.GetTileAtWorldPos(mouseWorldPos);

            if (tile != null && IsInRange(tile.gridPosition))
            {
                if (!hasHoveredTile || hoveredGridPos != tile.gridPosition)
                {
                    hoveredGridPos = tile.gridPosition;
                    hasHoveredTile = true;
                    OnTileHovered?.Invoke(hoveredGridPos);
                }
            }
            else
            {
                if (hasHoveredTile)
                {
                    hasHoveredTile = false;
                    LogDebug("Hover cleared");
                }
            }
        }

         private bool IsInRange(Vector2Int gridPos)
        {
            Vector3 tileWorldPos = farmGrid.GetTileWorldCenter(gridPos);
            float distance = Vector2.Distance(transform.position, tileWorldPos);
            return distance <= interactionRange;
        }
    
        
        // ==========================================
        // INPUT HANDLING
        // ==========================================
        
        private void HandleInput()
        {
            // Left Click - Use tool/item
            if (Input.GetMouseButtonDown(0))
            {
                if (hasHoveredTile)
                {
                    InteractWithTile(hoveredGridPos);
                }
            }
            
            // Right Click - Select tile
            if (Input.GetMouseButtonDown(1))
            {
                if (hasHoveredTile)
                {
                    SelectTile(hoveredGridPos);
                }
            }
        }

        private void SelectTile(Vector2Int gridPos)
        {
           selectedGridPos = gridPos;
            OnTileSelected?.Invoke(gridPos);
            
            FarmTile tile = farmGrid.GetTile(gridPos);
            LogDebug($"Selected tile: {gridPos}, State: {tile?.State}, HasCrop: {tile?.HasCrop}");
        }

        // ==========================================
        // MAIN INTERACTION
        // ==========================================

        private void InteractWithTile(Vector2Int gridPos)
        {
            if (farmGrid == null)
            {
                LogDebug("Cannot interact: FarmGrid is null");
                return;
            }
            
            if (!farmGrid.IsValidTile(gridPos))
            {
                LogDebug($"Cannot interact: Invalid tile {gridPos}");
                return;
            }
            
            if (animController != null && animController.IsActionLocked)
            {
                LogDebug("Cannot interact: animation locked");
                return;
            }
            
            bool success = false;
            
            // Check if holding item (Seeds)
            if (itemHolding != null && itemHolding.IsHoldingItem)
            {
                success = TryPlantFromHolding(gridPos);
            }
            // Check if holding tool
            else if (toolEquipment != null && toolEquipment.HasToolEquipped)
            {
                success = UseToolOnTile(gridPos);
            }
            // No tool/item - try harvest by hand
            else
            {
                success = TryHarvest(gridPos);
            }
            
            if (success)
            {
                OnTileInteracted?.Invoke(gridPos);
            }
        }
        // ==========================================
        // TOOL USAGE
        // ==========================================

        private bool UseToolOnTile(Vector2Int gridPos)
        {
            ToolType currentTool = toolEquipment.CurrentToolType;
            
            LogDebug($"Using tool {currentTool} on tile {gridPos}");
            
            bool success = false;
            
            switch (currentTool)
            {
                case ToolType.Hoe:
                    success = TryTill(gridPos);
                    break;
                    
                case ToolType.Watering:
                    success = TryWater(gridPos);
                    break;
                    
                case ToolType.Sickle:
                    success = TryHarvest(gridPos);
                    break;
                    
                default:
                    LogDebug($"Tool {currentTool} not implemented for farming");
                    break;
            }
            
            // Play tool animation if action succeeded
            if (success && toolEquipment != null)
            {
                toolEquipment.UseTool();
            }
            
            return success;
        }
        
        // ==========================================
        // FARMING ACTIONS
        // ==========================================
        
        /// Till the tile (make it ready for planting)
        private bool TryTill(Vector2Int gridPos)
        {
            bool success = farmGrid.TillTile(gridPos);
            
            if (success)
            {
                LogDebug($"Tilled tile at {gridPos}");
            }
            else
            {
                LogDebug($"Cannot till tile at {gridPos}");
            }
            
            return success;
        }

        /// Water the tile
        private bool TryWater(Vector2Int gridPos)
        {
            bool success = farmGrid.WaterTile(gridPos);

            var tile = farmGrid.GetTile(gridPos);
            if (tile.HasCrop)
            {
                tile.currentCrop.SetWatered(true);
            }

            if (success)
            {
                LogDebug($"Watered tile at {gridPos}");
            }
            else
            {
                LogDebug($"Cannot water tile at {gridPos}");
            }
            
            return success;
        }

        // Plant seed from ItemHoldingController
        private bool TryPlantFromHolding(Vector2Int gridPos)
        {
            // Get tile data
            FarmTile tile = farmGrid.GetTile(gridPos);
            if (tile == null || !tile.CanPlant())
            {
                LogDebug($"Cannot plant: tile not ready (State: {tile?.State})");
                return false;
            }
            
            // Get seed from ItemHoldingController
            Item heldItem = itemHolding.CurrentItem;
            if (heldItem == null || heldItem.ItemData == null)
            {
                LogDebug("Cannot plant: no item held");
                return false;
            }
            
            // Check if item is seed (CropItemData)
            CropItemData seedData = heldItem.ItemData as CropItemData;
            if (seedData == null || seedData.cropData == null)
            {
                LogDebug($"Cannot plant: item {heldItem.ItemData.itemName} is not a seed");
                return false;
            }
            
            // Check season
            Season currentSeason = TimeManager.Instance?.GetCurrentSeason() ?? Season.Spring;
            if (!seedData.cropData.IsValidSeason(currentSeason))
            {
                LogDebug($"Cannot plant: {seedData.cropData.cropName} not valid in {currentSeason}");
                return false;
            }
            
            // Check và consume seed từ inventory
            if (inventoryBridge != null)
            {
                if (!inventoryBridge.TryConsumeSeed(heldItem))
                {
                    LogDebug("Failed to consume seed from inventory");
                    return false;
                }
            }

            // Plant the crop
            bool planted = farmGrid.PlantCrop(gridPos, seedData.cropData);

            if (planted)
            {
                var farmTile = farmGrid.GetTile(gridPos);
                if (tile.HasCrop && tile.currentCrop != null)
                {
                    CropGrowthManager.Instance.RegisterCrop(tile.currentCrop);
                }
            }
            
            if (planted)
            {
                LogDebug($"Planted {seedData.cropData.cropName} at {gridPos}");
                
                // TODO: Remove 1 seed from inventory
                // inventoryManager.RemoveItem(seedData.itemID, 1);
                
                return true;
            }
            else
            {
                LogDebug("Failed to plant crop");
                return false;
            }
        }

        /// Harvest crop from the tile
        private bool TryHarvest(Vector2Int gridPos)
        {
            FarmTile tile = farmGrid.GetTile(gridPos);
            if (tile == null || !tile.CanHarvest())
            {
                LogDebug($"Cannot harvest: tile not ready");
                return false;
            }

            // Harvest the crop
            var harvestResult = farmGrid.HarvestTile(gridPos);

            var farmTile = farmGrid.GetTile(gridPos);
            if (!tile.HasCrop || !tile.currentCrop.Data.isRegrowable)
            {
                CropGrowthManager.Instance.UnregisterCrop(tile.currentCrop);
            }

            if (harvestResult != null && harvestResult.Count > 0)
            {
                LogDebug($"Harvested {harvestResult.Count} items from tile at {gridPos}");

                // Add items to inventory
                if (inventoryBridge != null)
                {
                    inventoryBridge.AddHarvestedItems(harvestResult);
                }

                // TODO: Add harvested items to inventory
                foreach (var item in harvestResult)
                {
                    // inventoryManager.AddItem(item);
                }

                return true;
            }
            else
            {
                LogDebug("Harvest returned no items");
                return false;
            }
        }
        
        public void ResetTile(Vector2Int gridPos)
        {
            var tile = farmGrid.GetTile(gridPos);
            if (tile.HasCrop)
            {
                CropGrowthManager.Instance.UnregisterCrop(tile.currentCrop);
            }

            farmGrid.ResetTile(gridPos);
        }

        /// Apply fertilizer to the tile (called from external system)
        public bool ApplyFertilizer(Vector2Int gridPos, FertilizerType type)
        {
            FarmTile tile = farmGrid.GetTile(gridPos);
            if (tile == null || !tile.HasCrop)
            {
                LogDebug("Cannot fertilize: no crop planted");
                return false;
            }
            
            farmGrid.ApplyFertilizer(gridPos, type);
            LogDebug($"Applied {type} to {gridPos}");
            return true;
        }
        
        // ==========================================
        // PUBLIC API
        // ==========================================
        
        public Vector2Int GetHoveredGridPos()
        {
            return hoveredGridPos;
        }
        
        public Vector2Int GetSelectedGridPos()
        {
            return selectedGridPos;
        }
        
        public FarmTile GetHoveredTile()
        {
            return hasHoveredTile ? farmGrid?.GetTile(hoveredGridPos) : null;
        }
        
        public FarmTile GetSelectedTile()
        {
            return farmGrid?.GetTile(selectedGridPos);
        }

        // ==========================================
        // DEBUG
        // ==========================================

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[FarmingController] {message}");
            }
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            // Draw interaction range
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
            
            // Draw hovered tile
            if (hasHoveredTile && farmGrid != null)
            {
                Vector3 tileWorldPos = farmGrid.GetTileWorldCenter(hoveredGridPos);
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireCube(tileWorldPos, Vector3.one * 0.9f);
            }
            
            // Draw selected tile
            if (farmGrid != null)
            {
                Vector3 tileWorldPos = farmGrid.GetTileWorldCenter(selectedGridPos);
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireCube(tileWorldPos, Vector3.one * 0.95f);
            }
        }
        
        [ContextMenu("Debug - Log Selected Tile")]
        private void DebugLogSelectedTile()
        {
            FarmTile tile = GetSelectedTile();
            
            if (tile != null)
            {
                Debug.Log("=== SELECTED TILE ===");
                Debug.Log($"Grid Position: {selectedGridPos}");
                Debug.Log($"State: {tile.State}");
                Debug.Log($"Is Tilled: {tile.IsTilled}");
                Debug.Log($"Is Watered: {tile.IsWatered}");
                Debug.Log($"Has Crop: {tile.HasCrop}");
                
                if (tile.HasCrop && tile.currentCrop != null)
                {
                    var crop = tile.currentCrop;
                    Debug.Log($"Crop: {crop.Data.cropName}");
                    Debug.Log($"Stage: {crop.CurrentStage}/{crop.Data.growthStages}");
                    Debug.Log($"Is Harvestable: {crop.IsHarvestable}");
                }
            }
            else
            {
                Debug.Log("No tile selected or tile is null");
            }
        }
        
        [ContextMenu("Debug - Test Till Hovered")]
        private void DebugTestTill()
        {
            if (hasHoveredTile)
            {
                TryTill(hoveredGridPos);
            }
            else
            {
                Debug.Log("No hovered tile");
            }
        }
        
        [ContextMenu("Debug - Test Harvest Hovered")]
        private void DebugTestHarvest()
        {
            if (hasHoveredTile)
            {
                TryHarvest(hoveredGridPos);
            }
            else
            {
                Debug.Log("No hovered tile");
            }
        }
#endif
    }
}

