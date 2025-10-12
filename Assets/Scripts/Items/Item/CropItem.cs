using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace TinyFarm.Items
{
    [Serializable]
    public class CropItem : Item
    {
        private CropItemData cropData;

        public CropType CropType => cropData?.cropType ?? CropType.None;
        public int BasePrice => cropData.basePrice;
        public bool IsUsable => cropData?.isUsable ?? false;
        public int HealthRestore => cropData?.nutritionValue ?? 0;
        public int EnergyRestore => cropData?.energyValue ?? 0;

        public CropItem(CropItemData data) : base(data)
        {
            this.cropData = data;
        }

        // Ăn crop (nếu edible)
        public override bool Use()
        {
            if (!IsUsable)
            {
                return false;
            }

            // Giảm stack
            Stackable.RemoveFromStack(1);
            return true;
        }

        public override string ToString()
        {
            return $"{Name} ({BasePrice}g";
        }
    }
}

