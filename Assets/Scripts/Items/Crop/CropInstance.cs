using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TinyFarm.Items;
using TinyFarm.Farming;

namespace TinyFarm.Crops
{
    public class CropInstance
    {
        // Reference
        private readonly CropData cropData;
        private readonly SpriteRenderer cropRenderer;

        // Growth Tracking
        private int currentStage = 0;
        private int daysInStage = 0;
        private int plantedDay;
        public FertilizerType fertilizer;
        private bool isWateredToday;


        // Optional: sử dụng sau này
        private FertilizerType appliedFertilizer = FertilizerType.None;

        // Properties
        public bool IsHarvestable => IsFullyGrown;
        public bool IsFullyGrown => currentStage >= cropData.growthStages - 1;

        public CropData Data => cropData;
        public int CurrentStage => currentStage;

        public CropInstance(CropData data, SpriteRenderer renderer, int today)
        {
            cropData = data;
            cropRenderer = renderer;
            plantedDay = today;

            currentStage = 0;
            daysInStage = 0;
            isWateredToday = false;
            fertilizer = FertilizerType.None;

            UpdateSprite();
        }

        // ==========================================
        // GROWTH LOGIC (called mỗi khi sang ngày mới)
        // ==========================================

        public void OnDayUpdate()
        {
            if (!isWateredToday && cropData.requiresDailyWatering)
                return;

            daysInStage++;

            if (daysInStage >= cropData.daysPerStage[currentStage])
            {
                daysInStage = 0;
                currentStage = Mathf.Min(currentStage + 1, cropData.growthStages - 1);
                UpdateSprite();
            }

            isWateredToday = false;
        }

        // ==========================================
        // WATERING
        // ==========================================

        public void Water()
        {
            isWateredToday = true;
        }

        // ==========================================
        // HARVESTING
        // ==========================================
       
        public List<Item> Harvest()
        {
            if (!IsHarvestable) return null;

            List<Item> result = new List<Item>();

            int yield = Random.Range(cropData.minYield, cropData.maxYield + 1);
            result.Add(new Item(cropData.harvestItem, yield));

            if (cropData.isRegrowable)
                Regrow();
            else
                DestroyCrop();

            return result;
        }

        private void DestroyCrop()
        {
            if (cropRenderer != null)
                cropRenderer.sprite = null;
        }

        // ==========================================
        // REGROWABLE LOGIC
        // ==========================================

        public bool CanRegrow()
        {
            return cropData.isRegrowable;
        }

        public void Regrow()
        {
            currentStage = Mathf.Clamp(cropData.regrowDays, 0, cropData.growthStages - 1);
            daysInStage = 0;
            UpdateSprite();
        }

        // ==========================================
        // VISUAL UPDATE
        // ==========================================

        public void UpdateSprite()
        {
            if (cropRenderer == null || cropData.stageSprites == null)
                return;

            int stage = Mathf.Clamp(currentStage, 0, cropData.stageSprites.Length - 1);
            cropRenderer.sprite = cropData.stageSprites[stage];
        }

        // ==========================================
        // FERTILIZER (tạm thời để hook sau)
        // ==========================================

        public void ApplyFertilizer(FertilizerType type)
        {
            appliedFertilizer = type;
        }
    }
}

