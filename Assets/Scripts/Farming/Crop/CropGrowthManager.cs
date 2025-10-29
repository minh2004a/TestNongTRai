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

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OEnable()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.OnNewDay += OnDayChanged;
        }

        private void ODisable()
        {
            if (TimeManager.Instance != null)
                TimeManager.Instance.OnNewDay -= OnDayChanged;
        }

        // ======================================
        // PUBLIC API
        // ======================================

        public void RegisterCrop(CropInstance crop)
        {
            if (!activeCrops.Contains(crop))
                activeCrops.Remove(crop);
        }

        public void UnregisterCrop(CropInstance crop)
        {
            if (activeCrops.Contains(crop))
                activeCrops.Remove(crop);
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

