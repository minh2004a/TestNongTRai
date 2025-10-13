using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    [Serializable]
    public class CraftingMaterial : Item
    {
        private CraftingMaterialData materialData;

        //Properties
        public MaterialTier MaterialTier => materialData?.tier ?? Items.MaterialTier.None;
        public bool IsRare => materialData?.isRare ?? false;

        // Constructor
        public CraftingMaterial(CraftingMaterialData data) : base(data)
        {
            this.materialData = data;
        }

        public CraftingMaterial(CraftingMaterial other) : base(other)
        {
            this.materialData = other.materialData;
        }

        /// Lấy thông tin crafting
        public string GetCraftingInfo()
        {
            string rare = IsRare ? " [RARE]" : "";
            return $"{MaterialTier} Material (Tier {MaterialTier}){rare}";
        }

        public override string ToString()
        {
            return $"{Name} x{CurrentStack} (T{MaterialTier})";
        }
    }
}


