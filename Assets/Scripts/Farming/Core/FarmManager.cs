using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.Farming
{
    // Quản lý tất cả FarmTiles trong game
    // Handles: Tile tracking, day updates, season changes
    public class FarmManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private FarmGrid farmGrid;
        
        [Header("Settings")]
        [SerializeField] private bool debugMode = true;
        
        // Singleton
        public static FarmManager Instance { get; private set; }
        
        // Properties
        public bool IsInitialized { get; private set; }
        public CropData CropData;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeReferences();
        }
        
        private void Start()
        {
            IsInitialized = true;
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        
        private void InitializeReferences()
        {
            if (farmGrid == null)
                farmGrid = FarmGrid.Instance;
                
            if (farmGrid == null)
            {
                Debug.LogError("[FarmManager] FarmGrid not found!");
            }
        }
        
        // ==========================================
        // PUBLIC API - TILE ACCESS
        // ==========================================
        
        public FarmTile GetTile(Vector2Int gridPos)
        {
            return farmGrid?.GetTile(gridPos);
        }
        
        public FarmTile GetTileAtWorldPos(Vector3 worldPos)
        {
            return farmGrid?.GetTileAtWorldPos(worldPos);
        }
        
        public List<FarmTile> GetAllTiles()
        {
            return farmGrid?.GetAllTiles() ?? new List<FarmTile>();
        }
        
        public List<FarmTile> GetTilesWithCrops()
        {
            List<FarmTile> result = new List<FarmTile>();
            List<FarmTile> allTiles = GetAllTiles();
            
            foreach (var tile in allTiles)
            {
                if (tile != null && tile.HasCrop)
                {
                    result.Add(tile);
                }
            }
            
            return result;
        }
        
        public List<FarmTile> GetHarvestableTiles()
        {
            List<FarmTile> result = new List<FarmTile>();
            List<FarmTile> allTiles = GetAllTiles();
            
            foreach (var tile in allTiles)
            {
                if (tile != null && tile.CanHarvest())
                {
                    result.Add(tile);
                }
            }
            
            return result;
        }
        
        // ==========================================
        // PUBLIC API - TILE ACTIONS
        // ==========================================
        
        public bool TillTile(Vector2Int gridPos)
        {
            return farmGrid?.TillTile(gridPos) ?? false;
        }
        
        public bool WaterTile(Vector2Int gridPos)
        {
            return farmGrid?.WaterTile(gridPos) ?? false;
        }
        
        public bool PlantCrop(Vector2Int gridPos, CropData cropData)
        {
            return farmGrid?.PlantCrop(gridPos, cropData) ?? false;
        }
        
        public List<Item> HarvestTile(Vector2Int gridPos)
        {
            return farmGrid?.HarvestTile(gridPos);
        }
        
        public void ApplyFertilizer(Vector2Int gridPos, FertilizerType type)
        {
            farmGrid?.ApplyFertilizer(gridPos, type);
        }
        
        public void ResetTile(Vector2Int gridPos)
        {
            farmGrid?.ResetTile(gridPos);
        }
        
        public void ResetAllTiles()
        {
            farmGrid?.ResetAllTiles();
            LogDebug("Reset all tiles");
        }
        
        // ==========================================
        // STATISTICS
        // ==========================================
        
        public int GetTotalTiles()
        {
            return GetAllTiles().Count;
        }
        
        public int GetPlantedTiles()
        {
            return GetTilesWithCrops().Count;
        }
        
        public int GetHarvestableTilesCount()
        {
            return GetHarvestableTiles().Count;
        }
        
        public int GetEmptyTiles()
        {
            int count = 0;
            foreach (var tile in GetAllTiles())
            {
                if (tile.State == TileState.Empty)
                    count++;
            }
            return count;
        }

        public int GetTilledTiles()
        {
            int count = 0;
            foreach (var tile in GetAllTiles())
            {
                if (tile.State == TileState.Tilled)
                    count++;
            }
            return count;
        }
        
        // ==========================================
        // GRID ACCESSORS (Proxy to FarmGrid)
        // ==========================================

        public SpriteRenderer GetCropRendererAt(Vector2Int gridPos)
        {
            return farmGrid != null ? farmGrid.GetCropRendererAt(gridPos) : null;
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            return farmGrid != null ? farmGrid.GridToWorld(gridPos) : Vector3.zero;
        }

        // ==========================================
        // UTILITIES
        // ==========================================
        
        public Vector3 GetTileWorldCenter(Vector2Int gridPos)
        {
            return farmGrid?.GetTileWorldCenter(gridPos) ?? Vector3.zero;
        }
        
        public bool IsValidTile(Vector2Int gridPos)
        {
            return farmGrid?.IsValidTile(gridPos) ?? false;
        }
        
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[FarmManager] {message}");
            }
        }
        
#if UNITY_EDITOR
        [ContextMenu("Debug - Log Statistics")]
        private void DebugLogStatistics()
        {
            Debug.Log("=== FARM STATISTICS ===");
            Debug.Log($"Total Tiles: {GetTotalTiles()}");
            Debug.Log($"Empty Tiles: {GetEmptyTiles()}");
            Debug.Log($"Tilled Tiles: {GetTilledTiles()}");
            Debug.Log($"Planted Tiles: {GetPlantedTiles()}");
            Debug.Log($"Harvestable Tiles: {GetHarvestableTiles()}");
        }
        
        [ContextMenu("Debug - Reset All")]
        private void DebugResetAll()
        {
            ResetAllTiles();
            Debug.Log("[FarmManager] Reset all tiles");
        }
#endif
    }
}

