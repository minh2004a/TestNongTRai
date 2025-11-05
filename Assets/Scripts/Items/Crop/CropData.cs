using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    [CreateAssetMenu(fileName = "CropData", menuName = "Game/Crop Data")]
    public class CropData : ScriptableObject
    {
        [Header("Basic")]
        public string cropID;
        public string cropName;
        public CropType cropType;
        public string fertilizerType;
        public float totalGrowTime;
        public float currentGrowTime;

        [Header("Growth")]
        public int growthStages = 4;
        public int[] daysPerStage;  // Ví dụ: [1, 2, 3, 2]
        public bool requiresDailyWatering = true;
        public bool isRegrowable = false;
        public int regrowDays; // Khi thu hoạch xong stage cuối
        public int regrowStage = 0;

        [Header("Season Rules")]
        public Season[] allowedSeasons;

        [Header("Drops")]
        public CropItemData harvestItem;
        public int minYield = 1;
        public int maxYield = 1;

        [Header("Visuals")]
        public Sprite[] stageSprites; // phải có bằng growthStages

        [Header("Economy")]
        public int sellPrice = 10;

        // ======================
        // VALIDATION & HELPERS
        // ======================
        public bool IsValidSeason(Season season)
        {
            if (allowedSeasons == null) return false;
            foreach (var s in allowedSeasons)
            {
                if (s == season) return true;
            }
            return false;
        }

        public int GetGrowthDays()
        {
            int total = 0;
            foreach (var d in daysPerStage)
                total += d;
            return total;
        }

        public int GetYield()
        {
            if (maxYield <= minYield) return minYield;
            return Random.Range(minYield, maxYield + 1);
        }

        public Sprite GetStageSprite(int stage)
        {
            if (stageSprites == null || stage < 0 || stage >= stageSprites.Length)
                return null;
            return stageSprites[stage];
        }

        private void OnValidate()
        {
            if (growthStages < 1)
            {
                growthStages = 1;
                Debug.LogWarning($"{cropName}: growthStages phải >= 1");
            }

            if (daysPerStage == null || daysPerStage.Length != growthStages)
            {
                Debug.LogWarning($"{cropName}: daysPerStage phải = growthStages");
                System.Array.Resize(ref daysPerStage, growthStages);
                for (int i = 0; i < growthStages; i++)
                    if (daysPerStage[i] < 1) daysPerStage[i] = 1;
            }

            if (stageSprites != null && stageSprites.Length != growthStages)
            {
                Debug.LogWarning($"{cropName}: stageSprites phải = growthStages");
            }

            if (string.IsNullOrEmpty(cropID))
                cropID = cropName.Replace(" ", "_").ToLower();
        }
    }
}

