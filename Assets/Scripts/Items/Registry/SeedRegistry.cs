using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    public class SeedRegistry : MonoBehaviour
    {
        [SerializeField] private ItemDatabase itemDatabase;

        // Mapping: seedItemID -> cropItemID
        [SerializeField] private Dictionary<string, string> seedToCropMap;

        // Mapping: cropItemID -> seedItemID (reverse lookup)
        [SerializeField] private Dictionary<string, string> cropToSeedMap;

        // Mapping: seedItemID -> SeedItemData
        [SerializeField] private Dictionary<string, SeedItemData> seedDataMap;

        [SerializeField] private bool isInitialized = false;


        public bool IsInitialized => isInitialized;
        public int RegisteredSeedCount => seedToCropMap?.Count ?? 0;


        public event Action OnRegistryInitialized;
        public event Action<string, string> OnSeedRegistered; // (seedID, cropID)

        private void Awake()
        {
            InitializeRegistry();
        }

        // Khởi tạo registry từ ItemDatabase
        public void InitializeRegistry()
        {
            if (isInitialized) return;

            if (itemDatabase == null)
            {
                Debug.LogError("[SeedRegistry] ItemDatabase is null!");
                return;
            }

            seedToCropMap = new Dictionary<string, string>();
            cropToSeedMap = new Dictionary<string, string>();
            seedDataMap = new Dictionary<string, SeedItemData>();

            LoadAllSeeds();

            isInitialized = true;
            OnRegistryInitialized?.Invoke();

        }

        // Load tất cả seeds từ database và build mappings
        private void LoadAllSeeds()
        {
            // Lấy tất cả SeedItemData từ database
            List<ItemData> allItems = itemDatabase.GetAllItems();

            foreach (var itemData in allItems)
            {
                if (itemData is SeedItemData seedData)
                {
                    RegisterSeed(seedData);
                }
            }
        }

        // Đăng ký seed vào registry
        private void RegisterSeed(SeedItemData seedData)
        {
            if (seedData == null) return;

            string seedID = seedData.itemID;

            // Lấy cropID từ resultCrop
            string cropID = seedData.resultCrop != null ? seedData.resultCrop.itemID : null;

            if (string.IsNullOrEmpty(cropID))
            {
                return;
            }

            // Build mappings
            seedToCropMap[seedID] = cropID;
            cropToSeedMap[cropID] = seedID;
            seedDataMap[seedID] = seedData;

            OnSeedRegistered?.Invoke(seedID, cropID);
        }

        // Lấy crop ID từ seed ID
        public string GetCropIDFromSeed(string seedID)
        {
            if (!isInitialized) InitializeRegistry();

            if (seedToCropMap.ContainsKey(seedID))
            {
                return seedToCropMap[seedID];
            }

            Debug.LogWarning($"[SeedRegistry] Seed '{seedID}' not found!");
            return null;
        }

        // Lấy seed ID từ crop ID
        public string GetSeedIDFromCrop(string cropID)
        {
            if (!isInitialized) InitializeRegistry();

            if (cropToSeedMap.ContainsKey(cropID))
            {
                return cropToSeedMap[cropID];
            }

            Debug.LogWarning($"[SeedRegistry] Crop '{cropID}' has no corresponding seed!");
            return null;
        }

        // Lấy SeedItemData từ seedID
        public SeedItemData GetSeedData(string seedID)
        {
            if (!isInitialized) InitializeRegistry();

            if (seedDataMap.ContainsKey(seedID))
            {
                return seedDataMap[seedID];
            }

            return null;
        }

        // Lấy CropItemData từ seedID
        public CropItemData GetCropDataFromSeed(string seedID)
        {
            string cropID = GetCropIDFromSeed(seedID);
            if (string.IsNullOrEmpty(cropID)) return null;

            ItemData itemData = itemDatabase.GetItem<CropItemData>(cropID);
            return itemData as CropItemData;
        }

        // Kiểm tra seed ID có tồn tại không
        public bool IsSeedRegistered(string seedID)
        {
            if (!isInitialized) InitializeRegistry();
            return seedToCropMap.ContainsKey(seedID);
        }

        // Kiểm tra crop có seed tương ứng không
        public bool HasCorrespondingSeed(string cropID)
        {
            if (!isInitialized) InitializeRegistry();
            return cropToSeedMap.ContainsKey(cropID);
        }

        // Tạo SeedItem từ seedID
        public SeedItem CreateSeedItem(string seedID, int quantity = 1)
        {
            SeedItemData seedData = GetSeedData(seedID);
            if (seedData == null)
            {
                Debug.LogError($"[SeedRegistry] Cannot create seed '{seedID}' - data not found!");
                return null;
            }

            return new SeedItem(seedData);
        }

        // Lấy tất cả seeds theo season
        public List<SeedItemData> GetSeedsBySeason(Season season)
        {
            if (!isInitialized) InitializeRegistry();

            List<SeedItemData> result = new List<SeedItemData>();

            foreach (var seedData in seedDataMap.Values)
            {
                if (seedData.validSeason == season)
                {
                    result.Add(seedData);
                }
            }

            return result;
        }

        // Lấy tất cả seeds theo CropType
        public List<SeedItemData> GetSeedsByCropType(CropType cropType)
        {
            if (!isInitialized) InitializeRegistry();

            List<SeedItemData> result = new List<SeedItemData>();

            foreach (var seedData in seedDataMap.Values)
            {
                if (seedData.cropType == cropType)
                {
                    result.Add(seedData);
                }
            }

            return result;
        }

        // Khi plant seed, trả về crop sẽ grow
        public CropItemData GetResultCrop(SeedItem seedItem)
        {
            if (seedItem == null) return null;

            string cropID = GetCropIDFromSeed(seedItem.ItemData.itemID);
            if (string.IsNullOrEmpty(cropID)) return null;

            ItemData itemData = itemDatabase.GetItem<CropItemData>(cropID);
            return itemData as CropItemData;
        }

        // Lấy growth time của seed
        public int GetGrowthTime(string seedID)
        {
            SeedItemData seedData = GetSeedData(seedID);
            return seedData != null ? seedData.growthDays : 0;
        }

        // Lấy yield range của seed
        public (int min, int max) GetYieldRange(string seedID)
        {
            SeedItemData seedData = GetSeedData(seedID);
            if (seedData != null)
            {
                return (seedData.minYield, seedData.maxYield);
            }
            return (0, 0);
        }

        [ContextMenu("Refresh Registry")]
        public void RefreshRegistry()
        {
            isInitialized = false;
            InitializeRegistry();
            Debug.Log("[SeedRegistry] Registry refreshed!");
        }

        [ContextMenu("Debug Registry")]
        private void DebugRegistry()
        {
            Debug.Log("=== SEED REGISTRY ===");
            Debug.Log($"Total Seeds: {RegisteredSeedCount}");

            foreach (var kvp in seedToCropMap)
            {
                Debug.Log($"{kvp.Key} → {kvp.Value}");
            }
        }

    }

}
