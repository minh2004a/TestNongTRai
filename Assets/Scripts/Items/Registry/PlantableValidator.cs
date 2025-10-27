using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace TinyFarm.Items
{
    public class PlantableValidator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SeedRegistry seedRegistry;

        [Header("Season Settings")]
        [SerializeField] private Season currentSeason = Season.None;
        [SerializeField] private bool strictSeasonCheck = true;

        public Season CurrentSeason
        {
            get => currentSeason;
            set
            {
                if (currentSeason != value)
                {
                    var old = currentSeason;
                    currentSeason = value;
                    OnSeasonChanged?.Invoke(old, currentSeason);
                }
            }
        }

        // Events
        public event Action<Season, Season> OnSeasonChanged;
        public event Action<SeedItem, string> OnValidationFailed;

        private void Awake()
        {
            if (seedRegistry == null)
                seedRegistry = FindObjectOfType<SeedRegistry>();
        }

        // =======================================================
        // MAIN VALIDATION LOGIC
        // =======================================================

        public bool CanPlant(SeedItem seed, out string reason)
        {
            reason = string.Empty;

            if (seed == null)
            {
                reason = "Seed is null.";
                return false;
            }

            if (seed.CurrentStack <= 0)
            {
                reason = "No seeds left.";
                OnValidationFailed?.Invoke(seed, reason);
                return false;
            }

            if (strictSeasonCheck && !seed.SeedData.CanPlantInSeason(currentSeason))
            {
                reason = $"Cannot plant in {currentSeason}.";
                OnValidationFailed?.Invoke(seed, reason);
                return false;
            }

            return true;
        }

        public bool CanPlant(SeedItem seed) => CanPlant(seed, out _);

        // =======================================================
        // UTILITY
        // =======================================================

        public List<SeedItemData> GetPlantableSeeds()
        {
            if (seedRegistry == null || !seedRegistry.IsInitialized)
                return new List<SeedItemData>();

            return seedRegistry.GetSeedsBySeason(currentSeason);
        }

        public int GetPlantableSeedCount() => GetPlantableSeeds().Count;

        public void SetSeason(Season season) => CurrentSeason = season;

        public void AdvanceSeason()
        {
            int total = Enum.GetValues(typeof(Season)).Length;
            CurrentSeason = (Season)(((int)currentSeason + 1) % total);
        }
    }
}

