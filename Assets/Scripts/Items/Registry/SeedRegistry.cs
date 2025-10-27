using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    public class SeedRegistry : MonoBehaviour
    {
        [SerializeField] private ItemDatabase itemDatabase;

        // Mapping: seedID -> SeedItemData
        private Dictionary<string, SeedItemData> seedDataMap = new();

        private bool isInitialized = false;

        public bool IsInitialized => isInitialized;
        public int RegisteredSeedCount => seedDataMap?.Count ?? 0;

        public event Action OnRegistryInitialized;

        private void Awake()
        {
            InitializeRegistry();
        }

        // ===============================================
        // INITIALIZATION
        // ===============================================

        public void InitializeRegistry()
        {
            if (isInitialized) return;

            if (itemDatabase == null)
            {
                Debug.LogError("[SeedRegistry] ItemDatabase is null!");
                return;
            }

            seedDataMap.Clear();
            LoadAllSeeds();

            isInitialized = true;
            OnRegistryInitialized?.Invoke();
        }

        private void LoadAllSeeds()
        {
            List<ItemData> allItems = itemDatabase.GetAllItems();

            foreach (var itemData in allItems)
            {
                if (itemData is SeedItemData seedData && seedData.cropData != null)
                {
                    string seedID = seedData.itemID;
                    if (!seedDataMap.ContainsKey(seedID))
                        seedDataMap.Add(seedID, seedData);
                }
            }

            Debug.Log($"[SeedRegistry] Loaded {seedDataMap.Count} seeds.");
        }

        // ===============================================
        // API
        // ===============================================

        public SeedItemData GetSeedData(string seedID)
        {
            if (!isInitialized) InitializeRegistry();
            seedDataMap.TryGetValue(seedID, out var data);
            return data;
        }

        public List<SeedItemData> GetSeedsBySeason(Season season)
        {
            if (!isInitialized) InitializeRegistry();

            List<SeedItemData> result = new();
            foreach (var seed in seedDataMap.Values)
            {
                if (seed.CanPlantInSeason(season))
                    result.Add(seed);
            }

            return result;
        }

        public List<SeedItemData> GetSeedsByCropType(CropType type)
        {
            if (!isInitialized) InitializeRegistry();

            List<SeedItemData> result = new();
            foreach (var seed in seedDataMap.Values)
            {
                if (seed.cropData != null && seed.cropData.cropType == type)
                    result.Add(seed);
            }

            return result;
        }

        public bool IsSeedRegistered(string seedID)
        {
            if (!isInitialized) InitializeRegistry();
            return seedDataMap.ContainsKey(seedID);
        }

        public SeedItem CreateSeedItem(string seedID)
        {
            var data = GetSeedData(seedID);
            if (data == null)
            {
                Debug.LogWarning($"[SeedRegistry] SeedID '{seedID}' not found.");
                return null;
            }

            return new SeedItem(data);
        }

#if UNITY_EDITOR
        [ContextMenu("Debug Registry")]
        private void DebugRegistry()
        {
            Debug.Log("=== SEED REGISTRY ===");
            Debug.Log($"Total Seeds: {RegisteredSeedCount}");
            foreach (var kvp in seedDataMap)
                Debug.Log($"- {kvp.Key} ({kvp.Value.itemName})");
        }
#endif
    }

}
