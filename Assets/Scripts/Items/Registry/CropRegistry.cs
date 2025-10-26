using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    public class CropRegistry : MonoBehaviour
    {
        [SerializeField] private ItemDatabase itemDatabase;

        // Mapping: cropItemID -> CropItemData
        [SerializeField] private Dictionary<string, CropItemData> cropDataMap;

        // Mapping: CropType -> List of cropIDs
        [SerializeField] private Dictionary<CropType, List<string>> cropsByType;

        // Mapping: Season -> List of cropIDs
        [SerializeField] private Dictionary<Season, List<string>> cropsBySeason;

        [SerializeField] private bool isInitialized = false;


        public bool IsInitialized => isInitialized;
        public int RegisteredCropCount => cropDataMap?.Count ?? 0;


        public event Action OnRegistryInitialized;
        public event Action<string> OnCropRegistered;

        private void Awake()
        {
            InitializeRegistry();
        }


        public void InitializeRegistry()
        {
            if (isInitialized) return;

            if (itemDatabase == null)
            {
                Debug.LogError("[CropRegistry] ItemDatabase is null!");
                return;
            }

            cropDataMap = new Dictionary<string, CropItemData>();
            cropsByType = new Dictionary<CropType, List<string>>();
            cropsBySeason = new Dictionary<Season, List<string>>();

            // Initialize dictionaries
            foreach (CropType type in System.Enum.GetValues(typeof(CropType)))
            {
                cropsByType[type] = new List<string>();
            }

            foreach (Season season in System.Enum.GetValues(typeof(Season)))
            {
                cropsBySeason[season] = new List<string>();
            }

            LoadAllCrops();

            isInitialized = true;
            OnRegistryInitialized?.Invoke();

        }

        private void LoadAllCrops()
        {
            List<ItemData> allItems = itemDatabase.GetAllItems();

            foreach (var itemData in allItems)
            {
                if (itemData is CropItemData cropData)
                {
                    RegisterCrop(cropData);
                }
            }
        }

        private void RegisterCrop(CropItemData cropData)
        {
            if (cropData == null) return;

            string cropID = cropData.itemID;

            // Add to main map
            cropDataMap[cropID] = cropData;

            // Add to type map
            if (cropsByType.ContainsKey(cropData.cropType))
            {
                cropsByType[cropData.cropType].Add(cropID);
            }

            // Add to season map
            if (cropsBySeason.ContainsKey(cropData.harvestedSeason))
            {
                cropsBySeason[cropData.harvestedSeason].Add(cropID);
            }

            OnCropRegistered?.Invoke(cropID);
        }

        // Lấy CropItemData từ cropID
        public CropItemData GetCropData(string cropID)
        {
            if (!isInitialized) InitializeRegistry();

            if (cropDataMap.ContainsKey(cropID))
            {
                return cropDataMap[cropID];
            }

            Debug.LogWarning($"[CropRegistry] Crop '{cropID}' not found!");
            return null;
        }

        /// Kiểm tra crop có tồn tại không
        public bool IsCropRegistered(string cropID)
        {
            if (!isInitialized) InitializeRegistry();
            return cropDataMap.ContainsKey(cropID);
        }

        /// Lấy tất cả crops theo CropType
        public List<CropItemData> GetCropsByType(CropType cropType)
        {
            if (!isInitialized) InitializeRegistry();

            List<CropItemData> result = new List<CropItemData>();

            if (cropsByType.ContainsKey(cropType))
            {
                foreach (string cropID in cropsByType[cropType])
                {
                    if (cropDataMap.ContainsKey(cropID))
                    {
                        result.Add(cropDataMap[cropID]);
                    }
                }
            }

            return result;
        }

        /// Lấy tất cả crops theo Season
        public List<CropItemData> GetCropsBySeason(Season season)
        {
            if (!isInitialized) InitializeRegistry();

            List<CropItemData> result = new List<CropItemData>();

            if (cropsBySeason.ContainsKey(season))
            {
                foreach (string cropID in cropsBySeason[season])
                {
                    if (cropDataMap.ContainsKey(cropID))
                    {
                        result.Add(cropDataMap[cropID]);
                    }
                }
            }

            return result;
        }

        /// Tạo CropItem từ cropID
        public CropItem CreateCropItem(string cropID, int quantity = 1)
        {
            CropItemData cropData = GetCropData(cropID);
            if (cropData == null)
            {
                Debug.LogError($"[CropRegistry] Cannot create crop '{cropID}' - data not found!");
                return null;
            }

            return new CropItem(cropData);
        }

        /// Lấy thông tin dinh dưỡng của crop
        public (int nutrition, int energy) GetNutritionInfo(string cropID)
        {
            CropItemData cropData = GetCropData(cropID);
            if (cropData != null)
            {
                return (cropData.nutritionValue, cropData.energyValue);
            }
            return (0, 0);
        }

        /// Kiểm tra crop có thể ăn không
        public bool IsEdible(string cropID)
        {
            CropItemData cropData = GetCropData(cropID);
            return cropData != null && (cropData.nutritionValue > 0 || cropData.energyValue > 0);
        }

        /// Kiểm tra crop có thể chế biến không
        public bool CanBeProcessed(string cropID)
        {
            CropItemData cropData = GetCropData(cropID);
            return cropData != null && cropData.CanBeProcessed();
        }

        /// Lấy giá trị bán của crop
        public int GetSellPrice(string cropID)
        {
            CropItemData cropData = GetCropData(cropID);
            return cropData != null ? cropData.sellPrice : 0;
        }

        /// Lấy crop type
        public CropType GetCropType(string cropID)
        {
            CropItemData cropData = GetCropData(cropID);
            return cropData != null ? cropData.cropType : CropType.None;
        }

        [ContextMenu("Refresh Registry")]
        public void RefreshRegistry()
        {
            isInitialized = false;
            InitializeRegistry();
            Debug.Log("[CropRegistry] Registry refreshed!");
        }

        [ContextMenu("Debug Registry")]
        private void DebugRegistry()
        {
            Debug.Log("=== CROP REGISTRY ===");
            Debug.Log($"Total Crops: {RegisteredCropCount}");

            foreach (CropType type in System.Enum.GetValues(typeof(CropType)))
            {
                if (cropsByType[type].Count > 0)
                {
                    Debug.Log($"{type}: {cropsByType[type].Count} crops");
                }
            }
        }
    }
}

