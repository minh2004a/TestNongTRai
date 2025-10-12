using UnityEngine;
using System.Collections.Generic;

namespace TinyFarm.Items
{
    [CreateAssetMenu(fileName = "New Material", menuName = "Game/Item/Crafting Material Data")]
    public class CraftingMaterialData : ItemData
    {
        [Header("Material Specific")]
        [Tooltip("Tier/độ hiếm của material")]
        public MaterialTier tier = MaterialTier.Common;

        [Header("Crafting Info")]
        [Tooltip("Danh sách recipe IDs sử dụng material này")]
        public List<string> usedInRecipes = new List<string>();

        [Tooltip("Material hiếm không?")]
        public bool isRare = false;

        [Header("Processing")]
        [Tooltip("Có thể refined/chế biến không?")]
        public bool canBeRefined = false;

        [Tooltip("ID của material sau khi refine")]
        public string refinedMaterialID;

        [Tooltip("Số lượng cần để refine")]
        [Range(1, 10)]
        public int refineRequiredAmount = 5;

        public override ItemType GetItemType()
        {
            return ItemType.CraftingMaterial;
        }

        protected override void ValidateItemData()
        {
            base.ValidateItemData();

            // Material có thể stack
            isStackable = true;

            // Material không thể ăn hoặc trang bị
            isUsable = false;
            canBeEquippable = false;

            // Tier cao thì hiếm
            if (tier >= MaterialTier.Rare)
            {
                isRare = true;
            }
        }

        // Lấy tier
        public MaterialTier GetTier()
        {
            return tier;
        }

        // Lấy danh sách recipes sử dụng material này
        public List<string> GetRecipes()
        {
            return new List<string>(usedInRecipes);
        }

        // Material có hiếm không?
        public bool IsRare()
        {
            return isRare;
        }

        // Có thể refine không?
        public bool CanBeRefined()
        {
            return canBeRefined && !string.IsNullOrEmpty(refinedMaterialID);
        }

        /// Lấy màu theo tier (để hiển thị UI)
        public Color GetTierColor()
        {
            switch (tier)
            {
                case MaterialTier.Common:
                    return new Color(0.7f, 0.7f, 0.7f); // Gray
                case MaterialTier.Rare:
                    return new Color(0.2f, 0.5f, 1f); // Blue
                case MaterialTier.Epic:
                    return new Color(0.7f, 0.2f, 1f); // Purple
                case MaterialTier.Legendary:
                    return new Color(1f, 0.6f, 0f); // Orange
                default:
                    return Color.white;
            }
        }
    }
}

