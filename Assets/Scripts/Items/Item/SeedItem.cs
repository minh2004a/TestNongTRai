using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TinyFarm.Farming;
using UnityEngine;

namespace TinyFarm.Items
{
    public class SeedItem : Item
    {
        private SeedItemData seedData;

        // Properties
        public CropData CropData => seedData?.cropData;
        public bool RequiresDailyWatering => seedData != null && seedData.requiresDailyWatering;
        public SeedItemData SeedData => seedData;


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
        public bool TryPlant(FarmTile tile, Season currentSeason)
        {
            if (tile == null || seedData == null)
                return false;

            // Kiểm tra xem mùa hiện tại có hợp lệ không
            if (!seedData.CanPlantInSeason(currentSeason))
                return false;

            // Kiểm tra ô đất có thể trồng không
            if (!tile.CanPlant())
                return false;

            // Thực hiện trồng
            bool planted = tile.Plant(seedData.cropData);
            if (planted)
            {
                // Giảm số lượng hạt giống
                //RemoveFromStack(1);

                // Kích hoạt event
                OnSeedPlanted?.Invoke(this, tile.transform.position);
            }

            return planted;
        }

        public string GetCropInfo()
        {
            if (CropData == null)
                return "No crop data linked.";

            return $"{CropData.cropName} - grows in {CropData.GetGrowthDays()} days, " +
                   $"yield: {CropData.minYield}-{CropData.maxYield}";
        }

        public override string ToString()
        {
            return $"{Name} x{CurrentStack}";
        }
    }
}

