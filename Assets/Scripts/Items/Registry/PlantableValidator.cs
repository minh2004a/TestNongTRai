using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    public class PlantableValidator : MonoBehaviour
    {
        [SerializeField] private SeedRegistry seedRegistry;
        [SerializeField] private Season currentSeason = Season.None;

        [Header("Validation Settings")]
        [SerializeField] private bool strictSeasonCheck = true;
        //[SerializeField] private bool checkSoilQuality = false;
        //[SerializeField] private int minSoilQuality = 1;

        public Season CurrentSeason
        {
            get => currentSeason;
            set
            {
                if (currentSeason != value)
                {
                    Season oldSeason = currentSeason;
                    currentSeason = value;
                    OnSeasonChanged?.Invoke(oldSeason, currentSeason);
                }
            }
        }

        public event Action<Season, Season> OnSeasonChanged; // (oldSeason, newSeason)
        public event Action<SeedItem, string> OnValidationFailed; // (seed, reason)

        private void Awake()
        {
            if (seedRegistry == null)
            {
                seedRegistry = FindObjectOfType<SeedRegistry>();
            }
        }

        /// <summary>
        /// Kiểm tra seed có thể trồng không (full validation)
        /// </summary>
        public bool CanPlant(SeedItem seedItem, out string reason)
        {
            reason = string.Empty;

            if (seedItem == null)
            {
                reason = "Seed is null";
                return false;
            }

            // Check có seed trong tay không
            if (seedItem.CurrentStack <= 0)
            {
                reason = "No seeds available";
                OnValidationFailed?.Invoke(seedItem, reason);
                return false;
            }

            // Check season
            if (strictSeasonCheck && !ValidateSeasonSummary(seedItem, out reason))
            {
                OnValidationFailed?.Invoke(seedItem, reason);
                return false;
            }

            return true;
        }

        /// Kiểm tra seed có thể trồng không (simple)
        public bool CanPlant(SeedItem seedItem)
        {
            return CanPlant(seedItem, out _);
        }

        /// Validate season requirement
        public bool ValidateSeason(SeedItem seedItem, Season season)
        {
            if (seedItem == null) return false;
            return seedItem.CanPlantInSeason(season);
        }

        private bool ValidateSeasonSummary(SeedItem seedItem, out string reason)
        {
            if (seedItem.CanPlantInSeason(currentSeason))
            {
                reason = string.Empty;
                return true;
            }

            reason = $"Cannot plant in {currentSeason}. Required: {seedItem.GrowSeasons}";
            return false;
        }

        /// Lấy tất cả seeds có thể trồng trong season hiện tại
        public List<SeedItemData> GetPlantableSeeds()
        {
            if (seedRegistry == null || !seedRegistry.IsInitialized)
            {
                Debug.LogWarning("[PlantableValidator] SeedRegistry not initialized!");
                return new List<SeedItemData>();
            }

            return seedRegistry.GetSeedsBySeason(currentSeason);
        }

        /// Lấy tất cả seeds có thể trồng theo CropType
        public List<SeedItemData> GetPlantableSeedsByType(CropType cropType)
        {
            List<SeedItemData> allSeeds = GetPlantableSeeds();
            List<SeedItemData> result = new List<SeedItemData>();

            foreach (var seedData in allSeeds)
            {
                if (seedData.cropType == cropType)
                {
                    result.Add(seedData);
                }
            }

            return result;
        }

        /// Đếm số seeds có thể trồng
        public int GetPlantableSeedCount()
        {
            return GetPlantableSeeds().Count;
        }

        /// Set season hiện tại
        public void SetSeason(Season season)
        {
            CurrentSeason = season;
        }

        /// Advance season (next season)
        public void AdvanceSeason()
        {
            int seasonIndex = (int)currentSeason;
            int nextSeasonIndex = (seasonIndex + 1) % System.Enum.GetValues(typeof(Season)).Length;
            CurrentSeason = (Season)nextSeasonIndex;

            Debug.Log($"[PlantableValidator] Season changed to {currentSeason}");
        }

        /// Lấy warning message nếu không thể plant
        public string GetPlantWarning(SeedItem seedItem)
        {
            if (CanPlant(seedItem, out string reason))
            {
                return string.Empty;
            }

            return reason;
        }

        /// Kiểm tra xem seed có sắp hết season không
        public bool IsSeasonEnding(SeedItem seedItem, int daysRemaining)
        {
            if (seedItem == null) return false;

            // Nếu số ngày còn lại < growth time, warning
            return daysRemaining < seedItem.GrowthTime;
        }

        [ContextMenu("Debug Plantable Seeds")]
        private void DebugPlantableSeeds()
        {
            Debug.Log($"=== PLANTABLE SEEDS ({currentSeason}) ===");

            List<SeedItemData> plantableSeeds = GetPlantableSeeds();
            Debug.Log($"Total: {plantableSeeds.Count}");

            foreach (var seedData in plantableSeeds)
            {
                Debug.Log($"- {seedData.itemName} ({seedData.cropType})");
            }
        }
    }
}

