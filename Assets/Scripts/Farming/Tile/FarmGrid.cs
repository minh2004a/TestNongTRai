using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TinyFarm.Crops;
using UnityEngine.Tilemaps;
using TinyFarm.Items;

namespace TinyFarm.Farming
{
    // Quản lý Tilemap và farm tile data
/// Handles: Tile rendering, crop sprites, tile state tracking
    public class FarmGrid : MonoBehaviour
    {
        [Header("Tilemap References")]
        [SerializeField] private Tilemap GroundTilemap;           // Layer cho đất
        [SerializeField] private Tilemap TilledTilemap;           // Layer cho cây
        [SerializeField] private Grid grid;
        [SerializeField] private Transform cropContainer;
        [SerializeField] private Tilemap WaterTilemap;
        
        [Header("Tile Assets")]
        [SerializeField] private TileBase emptySoilTile;
        [SerializeField] private TileBase tilledSoilTile;
        [SerializeField] private TileBase wateredSoilTile;
        
        [Header("Farm Area")]
        [SerializeField] private Vector2Int farmOrigin = Vector2Int.zero;
        [SerializeField] private Vector2Int farmSize = new Vector2Int(100, 100);

        [Header("Settings")]
        [SerializeField] private bool debugMode = true;
        
        // Data storage
        private Dictionary<Vector2Int, FarmTile> farmTiles = new Dictionary<Vector2Int, FarmTile>();
        private Dictionary<Vector2Int, SpriteRenderer> cropRenderers = new Dictionary<Vector2Int, SpriteRenderer>();
        
        // Singleton
        public static FarmGrid Instance { get; private set; }
        public CropData CropData;
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            InitializeGrid();
        }
        
        private void Start()
        {
            SubscribeToTimeEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromTimeEvents();

            if (Instance == this)
                Instance = null;
        }

        // ==========================================
        // INITIALIZATION
        // ==========================================

        private void InitializeGrid()
        {
            if (grid == null)
                grid = GetComponentInParent<Grid>();

            if (GroundTilemap == null)
                GroundTilemap = GetComponent<Tilemap>();

            if (TilledTilemap == null)
            {
                // Try find sibling tilemap named maybe "Tilled" (optional)
                var maps = GetComponentsInChildren<Tilemap>();
                foreach (var m in maps)
                {
                    if (m != GroundTilemap)
                    {
                        TilledTilemap = m;
                        break;
                    }
                }
            }

            LogDebug($"InitializeGrid: origin={farmOrigin}, size={farmSize}, GroundTilemap={(GroundTilemap!=null)}, TilledTilemap={(TilledTilemap!=null)}, Grid={(grid!=null)}");
            // Initialize farm tiles
            for (int x = 0; x < farmSize.x; x++)
            {
                for (int y = 0; y < farmSize.y; y++)
                {
                    Vector2Int pos = farmOrigin + new Vector2Int(x, y);
                    farmTiles[pos] = new FarmTile(pos);

                    // Set initial tile
                    SetTilemapTile(pos, emptySoilTile, false);
                }
            }

            LogDebug($"Initialized grid with {farmTiles.Count} tiles");
        }
        
        // ==========================================
        // TIME EVENTS
        // ==========================================
        
        private void SubscribeToTimeEvents()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayStart += OnDayPassed;
                LogDebug("Subscribed to TimeManager");
            }
        }
        
        private void UnsubscribeFromTimeEvents()
        {
            if (TimeManager.Instance != null)
            {
                TimeManager.Instance.OnDayStart -= OnDayPassed;
            }
        }

        private void OnDayPassed()
        {
            LogDebug("Day passed - updating all tiles");

            foreach (var kvp in farmTiles)
            {
                kvp.Value.OnDayPassed();
                UpdateTileVisuals(kvp.Key);
            }
        }
        
        // ==========================================
        // TILE ACCESS
        // ==========================================
        
        public FarmTile GetTile(Vector2Int gridPos)
        {
            farmTiles.TryGetValue(gridPos, out FarmTile tile);
            return tile;
        }
        
        public FarmTile GetTileAtWorldPos(Vector3 worldPos)
        {
            if (GroundTilemap == null)
            {
                LogDebug($"GetTileAtWorldPos: GroundTilemap is NULL (worldPos={worldPos})");
                return null;
            }

            Vector3Int cellPos = GroundTilemap.WorldToCell(worldPos);
            Vector2Int gridPos = new Vector2Int(cellPos.x, cellPos.y);
            return GetTile(gridPos);
        }
        
        public bool IsValidTile(Vector2Int gridPos)
        {
            return farmTiles.ContainsKey(gridPos);
        }
        
        public List<FarmTile> GetAllTiles()
        {
            return new List<FarmTile>(farmTiles.Values);
        }

        public Vector3 GetTileWorldCenter(Vector2Int gridPos)
        {
            return GroundTilemap.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y, 0));
        }
        
        // ==========================================
        // TILE ACTIONS (Wrappers)
        // ==========================================
        
        public bool TillTile(Vector2Int gridPos)
        {
            LogDebug($"TillTile called: {gridPos}");
            FarmTile tile = GetTile(gridPos);
            if (tile == null || !tile.CanHoe())
            {
                LogDebug($"TillTile FAILED: no tile data at {gridPos}");
                return false;
            }

            LogDebug($"TillTile current state: {tile.tileState}, isTilled={tile.isTilled}");
            if (!tile.CanHoe())
            {
                LogDebug($"TillTile FAILED: CanHoe() returned false for {gridPos}");
                return false;
            }

            tile.Till();
            UpdateTileVisuals(gridPos);
            
            LogDebug($"Tilled tile at {gridPos}");
            return true;
        }
        
        public bool WaterTile(Vector2Int gridPos)
        {
            LogDebug($"WaterTile called: {gridPos}");
            FarmTile tile = GetTile(gridPos);
            if (tile == null || !tile.CanWater()) return false;
            
            if (!tile.CanWater())
            {
                LogDebug($"WaterTile FAILED: CanWater() returned false for {gridPos} (HasCrop={tile.HasCrop}, isWatered={tile.isWatered})");
                return false;
            }

            tile.Water();
            UpdateTileVisuals(gridPos);
            
            LogDebug($"Watered tile at {gridPos}");
            return true;
        }
        
        public bool PlantCrop(Vector2Int gridPos, CropData cropData)
        {
            LogDebug($"PlantCrop called: {gridPos}, crop={(cropData!=null?cropData.cropName:"NULL")}");
            FarmTile tile = GetTile(gridPos);
            if (tile == null || !tile.CanPlant()) return false;

            
            // Create crop renderer
            SpriteRenderer cropRenderer = GetOrCreateCropRenderer(gridPos);
            
            bool planted = tile.Plant(cropData, cropRenderer);
            
            if (planted)
            {
                UpdateTileVisuals(gridPos);
                LogDebug($"Planted {cropData.cropName} at {gridPos}");
            }
            
            return planted;
        }
        
        public List<Item> HarvestTile(Vector2Int gridPos)
        {
            FarmTile tile = GetTile(gridPos);
            if (tile == null || !tile.CanHarvest()) return null;

            var items = tile.Harvest();

            // If crop does not regrow → unregister + clear tile visuals
            if (!tile.HasCrop)
            {
                CropGrowthManager.Instance.UnregisterCrop(tile.currentCrop);
            }
            else
            {
                // Crop regrow → refresh sprite
                tile.currentCrop.UpdateSprite();
            }

            UpdateTileVisuals(gridPos);

            LogDebug($"Harvested tile at {gridPos}");
            return items;
        }
        
        public void ApplyFertilizer(Vector2Int gridPos, FertilizerType type)
        {
            FarmTile tile = GetTile(gridPos);
            if (tile == null) return;
            
            tile.ApplyFertilizer(type);
            LogDebug($"Applied {type} to {gridPos}");
        }

        public void ResetTile(Vector2Int gridPos)
        {
            FarmTile tile = GetTile(gridPos);
            if (tile == null) return;

            tile.ResetTile();
            UpdateTileVisuals(gridPos);

            // Destroy crop renderer
            if (cropRenderers.TryGetValue(gridPos, out SpriteRenderer renderer))
            {
                if (renderer != null)
                    Destroy(renderer.gameObject);
                cropRenderers.Remove(gridPos);
            }
        }
        
        // ==========================================
        // VISUAL UPDATES
        // ==========================================
        
        private void UpdateTileVisuals(Vector2Int gridPos)
        {
            var tile = GetTile(gridPos);
            if (tile == null) return;

            bool watered = tile.IsWatered;
            TileBase soilTile = tile.State == TileState.Empty ? emptySoilTile : tilledSoilTile;

            SetTilemapTile(gridPos, soilTile, watered);

            // Crop sprite update
            if (tile.currentCrop != null)
                tile.currentCrop.UpdateSprite();
        }
        
        private TileBase GetSoilTileForState(TileState state, bool isWatered)
        {
            if (isWatered)
                return wateredSoilTile;
                
            switch (state)
            {
                case TileState.Empty:
                    return emptySoilTile;
                case TileState.Tilled:
                case TileState.Planted:
                    return tilledSoilTile;
                case TileState.Watered:
                    return wateredSoilTile;
                default:
                    return emptySoilTile;
            }
        }

        private void SetTilemapTile(Vector2Int gridPos, TileBase soilTile, bool watered)
        {
            Vector3Int cellPos = new Vector3Int(gridPos.x, gridPos.y, 0);

            // GroundTilemap luôn giữ nguyên (đất gốc)
            
            if (soilTile == emptySoilTile)
            {
                TilledTilemap.SetTile(cellPos, null);
                WaterTilemap.SetTile(cellPos, null);
                return;
            }

            // Đặt tile xới
            TilledTilemap.SetTile(cellPos, tilledSoilTile);

            // Nếu tưới → thêm tile overlay
            if (watered)
                WaterTilemap.SetTile(cellPos, wateredSoilTile);
            else
                WaterTilemap.SetTile(cellPos, null);
        }

        // ==========================================
        // CROP RENDERER MANAGEMENT
        // ==========================================

        private SpriteRenderer GetOrCreateCropRenderer(Vector2Int gridPos)
        {
            // Check if already exists
            if (cropRenderers.TryGetValue(gridPos, out SpriteRenderer existing))
            {
                if (existing != null)
                    return existing;
            }

            // Create new renderer
            Vector3 worldPos = GetTileWorldCenter(gridPos);

            GameObject cropObj = new GameObject($"Crop_{gridPos.x}_{gridPos.y}");
            cropObj.transform.position = worldPos;
            cropObj.transform.SetParent(cropContainer);

            SpriteRenderer renderer = cropObj.AddComponent<SpriteRenderer>();
            renderer.sortingLayerName = "Crops"; // Hoặc layer bạn muốn
            renderer.sortingOrder = -gridPos.y;

            cropRenderers[gridPos] = renderer;

            return renderer;
        }

        public SpriteRenderer GetCropRendererAt(Vector2Int gridPos)
        {
            if (cropRenderers.TryGetValue(gridPos, out var renderer))
                return renderer;
            return null;
        }

        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            return GroundTilemap.GetCellCenterWorld(new Vector3Int(gridPos.x, gridPos.y, 0));
        }
        
        // ==========================================
        // UTILITIES
        // ==========================================
        
        public void ResetAllTiles()
        {
            foreach (var kvp in farmTiles)
            {
                ResetTile(kvp.Key);
            }
            
            LogDebug("Reset all tiles");
        }

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[FarmGrid] {message}");
            }
        }
        
        #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            // Draw farm area
            if (GroundTilemap != null && grid != null)
            {
                Gizmos.color = Color.green;
                
                for (int x = 0; x < farmSize.x; x++)
                {
                    for (int y = 0; y < farmSize.y; y++)
                    {
                        Vector2Int pos = farmOrigin + new Vector2Int(x, y);
                        Vector3 worldPos = grid.GetCellCenterWorld(new Vector3Int(pos.x, pos.y, 0));
                        Gizmos.DrawWireCube(worldPos, grid.cellSize);
                    }
                }
            }
        }
        
        [ContextMenu("Debug - Reset All Tiles")]
        private void DebugResetAll()
        {
            ResetAllTiles();
        }
        
        [ContextMenu("Debug - Log Tile Count")]
        private void DebugLogCount()
        {
            Debug.Log($"[FarmGrid] Total tiles: {farmTiles.Count}");
            
            int planted = 0;
            int harvestable = 0;
            
            foreach (var tile in farmTiles.Values)
            {
                if (tile.HasCrop) planted++;
                if (tile.CanHarvest()) harvestable++;
            }
            
            Debug.Log($"Planted: {planted}, Harvestable: {harvestable}");
        }
#endif
    }
}

