using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TinyFarm.Items;
using TinyFarm.Crops;

namespace TinyFarm.Farming
{
    public class FarmTile : MonoBehaviour
    {
        [Header("Tile State")]
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private TileState tileState = TileState.Empty;
        [SerializeField] private bool isWatered = false;
        [SerializeField] private bool isTilled = false;
        [SerializeField] private FertilizerType appliedFertilizer = FertilizerType.None;

        [Header("Crop")]
        [SerializeField] private CropInstance currentCrop;
        [SerializeField] private SpriteRenderer cropRenderer;

        [Header("Rendering")]
        [SerializeField] private SpriteRenderer tileRenderer;
        [SerializeField] private Sprite emptySoil;
        [SerializeField] private Sprite tilledSoil;
        [SerializeField] private Sprite wateredSoil;

        // PROPERTIES
        public Vector2Int GridPosition => gridPosition;
        public bool HasCrop => currentCrop != null;
        public CropInstance Crop => currentCrop;
        public bool IsWatered => isWatered;
        public bool IsTilled => isTilled;
        public TileState State => tileState;

        private void Awake()
        {
            if (tileRenderer == null)
                tileRenderer = GetComponent<SpriteRenderer>();

            UpdateVisuals();
        }

        // ==========================================================
        // TILE ACTIONS
        // ==========================================================

        public void Till()
        {
            if (tileState == TileState.Empty)
            {
                isTilled = true;
                tileState = TileState.Tilled;
                UpdateVisuals();
            }
        }

        public void Water()
        {
            if (!HasCrop) return;

            isWatered = true;
            tileState = TileState.Watered;
            currentCrop.Water();

            UpdateVisuals();
        }

        public bool Plant(CropData data)
        {
            if (!CanPlant() || data == null) return false;

            SpriteRenderer renderer = GetOrCreateCropRenderer();
            int today = TimeManager.Instance != null ? TimeManager.Instance.GetCurrentDay() : 0;

            currentCrop = new CropInstance(data, renderer, today);

            isTilled = true;
            isWatered = false;
            tileState = TileState.Planted;

            UpdateVisuals();
            return true;
        }

        private SpriteRenderer GetOrCreateCropRenderer()
        {
            if (cropRenderer != null) return cropRenderer;

            GameObject go = new GameObject("CropSprite");
            go.transform.SetParent(transform);
            go.transform.localPosition = Vector3.zero;

            cropRenderer = go.AddComponent<SpriteRenderer>();
            cropRenderer.sortingLayerID = tileRenderer.sortingLayerID;
            cropRenderer.sortingOrder = tileRenderer.sortingOrder + 1;

            return cropRenderer;
        }

        public void OnDayPassed()
        {
            currentCrop?.OnDayUpdate();
            UpdateVisuals();
        }

        public void ApplyFertilizer(FertilizerType type)
        {
            if (!HasCrop) return;
            appliedFertilizer = type;
            currentCrop.ApplyFertilizer(type);
        }

        public List<Item> Harvest()
        {
            if (!HasCrop) return null;
            if (!currentCrop.IsHarvestable) return null;

            var drops = currentCrop.Harvest();

            ResetCropData();
            UpdateVisuals();

            return drops;
        }

        public void ResetTile()
        {
            ResetCropData();
            isTilled = false;
            tileState = TileState.Empty;
            isWatered = false;

            UpdateVisuals();
        }

        private void ResetCropData()
        {
            currentCrop = null;
            appliedFertilizer = FertilizerType.None;
        }

        // ==========================================================
        // VALIDATIONS
        // ==========================================================

        public bool CanPlant()
        {
            return tileState == TileState.Tilled && !HasCrop;
        }

        public bool CanHoe() => tileState == TileState.Empty;
        public bool CanWater() => HasCrop && !isWatered;
        public bool CanHarvest() => HasCrop && currentCrop.IsHarvestable;

        // ==========================================================
        // VISUALS
        // ==========================================================

        public void UpdateVisuals()
        {
            if (tileRenderer == null) return;

            switch (tileState)
            {
                case TileState.Empty:
                    tileRenderer.sprite = emptySoil;
                    break;
                case TileState.Tilled:
                    tileRenderer.sprite = tilledSoil;
                    break;
                case TileState.Watered:
                    tileRenderer.sprite = wateredSoil;
                    break;
                default:
                    tileRenderer.sprite = tilledSoil;
                    break;
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (tileRenderer == null)
                tileRenderer = GetComponent<SpriteRenderer>();
        }
#endif
    }
}


