using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace TinyFarm.Items
{
    public class SeedItem : Item
    {
        private SeedItemData seedData;

        // Properties
        public CropType CropType => seedData?.cropType ?? CropType.None;
        public int GrowthTime => seedData?.growthDays ?? 0;
        public Season GrowSeasons => seedData?.validSeason ?? new Season();
        public int HarvestYield => UnityEngine.Random.Range(seedData.minYield, seedData.maxYield + 1);
        public CropItemData ResultCrop => seedData?.resultCrop;

        // Events
        public event Action<SeedItem, Vector3> OnSeedPlanted;

        public SeedItem(SeedItemData data) : base(data)
        {
            this.seedData = data;
        }

        public SeedItem(SeedItem other) : base(other)
        {
            this.seedData = other.seedData;
        }

        // Trồng seed tại vị trí
        public bool PlantSeed(Vector3 position, Season currentSeason)
        {
            // Kiểm tra season
            if (!CanPlantInSeason(currentSeason))
            {
                return false;
            }

            if (CurrentStack <= 0)
            {
                return false;
            }

            // Giảm số lượng seed
            Stackable.RemoveFromStack(1);

            OnSeedPlanted?.Invoke(this, position);
            return true;
        }

        // Kiểm tra có thể trồng trong season này không
        public bool CanPlantInSeason(Season season)
        {
            return GrowSeasons == season;
        }

        // Lấy thông tin crop sẽ thu hoạch
        public string GetCropInfo()
        {
            return $"Grows in {GrowthTime} days, Yields {HarvestYield} {ResultCrop?.itemName}";
        }

        public override string ToString()
        {
            return $"{Name} x{CurrentStack} (Growth: {GrowthTime}d)";
        }
    }
}

