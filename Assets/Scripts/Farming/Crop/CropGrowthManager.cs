using System.Collections;
using System.Collections.Generic;
using TinyFarm.Crops;
using UnityEngine;

namespace TinyFarm.Farming
{
    public class CropGrowthManager : MonoBehaviour
    {
        public static CropGrowthManager Instance { get; private set; }

        private readonly List<CropInstance> activeCrops = new List<CropInstance>();
        private List<FarmTile> trackedTiles = new List<FarmTile>();

        private void Awake()
        {
            if (Instance != null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void OnEnable()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.OnNewDay += OnDayChanged;
        }

        private void OnDisable()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.OnNewDay -= OnDayChanged;
        }

        // ======================================
        // PUBLIC API
        // ======================================

        // ===================================================
        // Register / Unregister
        // ===================================================
        public void RegisterCrop(CropInstance crop)
        {
            if (!activeCrops.Contains(crop))
                activeCrops.Add(crop);
        }

        public void UnregisterCrop(CropInstance crop)
        {
            if (activeCrops.Contains(crop))
                activeCrops.Remove(crop);
        }

        public void RegisterTile(FarmTile tile)
        {
            if (!trackedTiles.Contains(tile))
                trackedTiles.Add(tile);
        }

        public void UnregisterTile(FarmTile tile)
        {
            if (trackedTiles.Contains(tile))
                trackedTiles.Remove(tile);
        }

        // ======================================
        // DAILY GROWTH UPDATE
        // ======================================
        private void OnDayChanged()
        {
            ProcessDailyGrowth();
        }

        private void ProcessDailyGrowth()
        {
            if (activeCrops.Count == 0) return;

            foreach (var crop in activeCrops)
            {
                crop?.OnDayUpdate();
            }
        }
        
        // ======================================
        // FERTILIZER HOOK (future xài)
        // ======================================
        private void ApplyFertilizerEffects(CropInstance crop)
        {
            // TODO: Modify growth based on fertilizer type
        }
    }
}

