using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TinyFarm.Items;
using TinyFarm.Crops;

namespace TinyFarm.Farming
{
    // Data class cho mỗi tile trong farm grid
    [System.Serializable]
    public class FarmTile
    {
        // Position
        public Vector2Int gridPosition;
        public GroundType groundType;

        // State
        public TileState tileState = TileState.Empty;
        public bool isWatered = false;
        public bool isTilled = false;
        public FertilizerType appliedFertilizer = FertilizerType.None;

        // Crop
        public CropInstance currentCrop;

        // Added: cờ cho biết có vật đè lên ô này không
        public bool IsOccupied = false;

        // Properties
        public bool HasCrop => currentCrop != null;
        public bool IsWatered => isWatered;
        public bool IsTilled => isTilled;
        public TileState State => tileState;

        // Constructor
        public FarmTile(Vector2Int position)
        {
            gridPosition = position;
            tileState = TileState.Empty;
            isWatered = false;
            isTilled = false;
            currentCrop = null;
            appliedFertilizer = FertilizerType.None;
            groundType = GroundType.Empty;
            IsOccupied = false;
        }

        // ==========================================
        // TILE ACTIONS
        // ==========================================

        public void Till()
        {
            if (tileState == TileState.Empty)
            {
                isTilled = true;
                tileState = TileState.Tilled;
            }
        }

        public void Water()
        {
            if (!isTilled) return;

            isWatered = true;
            tileState = TileState.Watered;
            currentCrop?.Water();
        }

        public bool Plant(CropData data, SpriteRenderer cropRenderer)
        {
            if (!CanPlant() || data == null) return false;

            int today = TimeManager.Instance?.GetCurrentDay() ?? 0;
            currentCrop = new CropInstance(data, cropRenderer, today);

            isTilled = true;
            isWatered = false;
            tileState = TileState.Planted;

            return true;
        }

        public void OnDayPassed()
        {
            currentCrop?.OnDayUpdate();

            // Reset watered state daily
            if (isWatered)
            {
                isWatered = false;
                tileState = HasCrop ? TileState.Planted : TileState.Tilled;
            }
        }

        public void ApplyFertilizer(FertilizerType type)
        {
            if (!HasCrop) return;
            appliedFertilizer = type;
            currentCrop?.ApplyFertilizer(type);
        }

        public List<Item> Harvest()
        {
            if (!HasCrop) return null;
            if (!currentCrop.IsHarvestable) return null;

            var drops = currentCrop.Harvest();

            // If not regrowable, reset crop
            if (!currentCrop.CanRegrow())
            {
                ResetCropData();
            }

            return drops;
        }

        public void ResetTile()
        {
            ResetCropData();
            isTilled = false;
            tileState = TileState.Empty;
            isWatered = false;
        }

        private void ResetCropData()
        {
            currentCrop = null;
            appliedFertilizer = FertilizerType.None;
        }

        // ==========================================
        // VALIDATIONS
        // ==========================================

        public bool CanPlant()
        {
            return tileState == TileState.Tilled && !HasCrop;
        }

        public bool CanHoe()
        {
            return tileState == TileState.Empty && !IsOccupied;
        }
        public bool CanWater() => HasCrop && !isWatered;
        public bool CanHarvest() => HasCrop && currentCrop != null && currentCrop.IsHarvestable;
    }

    public enum GroundType
    {
        Empty,      // Đất trống có thể đào
        Grass,      // Thảm cỏ
        Rock,       // Đá
        Tree        // Cây hoặc chướng ngại vật
    }
}




