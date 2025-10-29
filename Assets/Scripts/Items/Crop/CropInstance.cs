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
        public FertilizerType fertilizer;
        private bool isWateredToday;
        private bool isInRegrowPhase = false;
        private int regrowDaysPassed = 0;


        // Optional: sử dụng sau này
        private FertilizerType appliedFertilizer = FertilizerType.None;

        // Properties
        public bool IsHarvestable => IsFullyGrown && !isInRegrowPhase;
        public bool IsFullyGrown => currentStage >= cropData.growthStages - 1;

        public CropData Data => cropData;
        public int CurrentStage => currentStage;

        public CropInstance(CropData data, SpriteRenderer renderer, int today)
        {
            cropData = data;
            cropRenderer = renderer;

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
            if (isInRegrowPhase)
            {
                if (!cropData.regrowNeedsWater || isWateredToday)
                {
                    regrowDaysPassed++;

                    if (regrowDaysPassed >= cropData.regrowDays)
                        FinishRegrow();
                }

                isWateredToday = false;
                return;
            }
            if (!isWateredToday && cropData.requiresDailyWatering)
                return;

            if (currentStage >= cropData.growthStages - 1 && !cropData.isRegrowable)
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

        public void SetWatered(bool watered)
        {
            isWateredToday = watered;
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
            {
                Regrow(); // ✅ chuyển sang stage regrow + update sprite
            }
            else
            {
                // ✅ hoàn toàn hủy crop
                DestroyCrop();
            }

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
            currentStage = Mathf.Clamp(cropData.regrowStartStage, 0, cropData.growthStages - 1);
            daysInStage = 0;
            isWateredToday = false; // ✅ cần tưới lại mới mọc tiếp

            UpdateSprite(); // ✅ cập nhật visual ngay lập tức
        }

        // ==========================================
        // VISUAL UPDATE
        // ==========================================

        public void FinishRegrow()
        {
            isInRegrowPhase = false;

            currentStage = Mathf.Clamp(cropData.growthStages - 1, 0, cropData.growthStages - 1);
            UpdateSprite();
        }

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

