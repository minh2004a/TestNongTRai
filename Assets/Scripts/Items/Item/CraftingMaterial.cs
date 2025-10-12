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

        // Properties
        //public MaterialTier MaterialTier => materialData?.materialTier ?? materialTier.None;
        //public int MaterialTier => materialData?.materialTier ?? 1;
        //public bool IsRare => materialData?.isRare ?? false;

        //// Constructor
        public CraftingMaterial(CraftingMaterialData data) : base(data)
        {
            this.materialData = data;
        }

        //public CraftingMaterial(CraftingMaterial other) : base(other)
        //{
        //    this.materialData = other.materialData;
        //}

        ///// Kiểm tra có thể dùng cho recipe không
        //public bool CanUseForRecipe(MaterialCategory requiredCategory, int requiredTier)
        //{
        //    if (MaterialCategory != requiredCategory)
        //        return false;

        //    return MaterialTier >= requiredTier;
        //}

        ///// Lấy thông tin crafting
        //public string GetCraftingInfo()
        //{
        //    string rare = IsRare ? " [RARE]" : "";
        //    return $"{MaterialCategory} Material (Tier {MaterialTier}){rare}";
        //}

        //public override string ToString()
        //{
        //    return $"{Name} x{CurrentStack} (T{MaterialTier})";
        //}
    }
}


