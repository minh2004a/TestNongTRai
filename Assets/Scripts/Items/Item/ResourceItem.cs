using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    [Serializable]
    public class ResourceItem : Item
    {
        private ResourcesItemData resourceData;

        // Properties
        public ResourcesType ResourceType => resourceData?.resourceType ?? ResourcesType.None;
        public bool IsRefinable => resourceData?.isRefinable ?? false;
        public ResourcesItemData RefinedResult => resourceData?.refinedResult;
        public int RefineAmount => resourceData?.refineAmount ?? 1;

        // Constructor
        public ResourceItem(ResourcesItemData data) : base(data)
        {
            this.resourceData = data;
        }

        public ResourceItem(ResourceItem other) : base(other)
        {
            this.resourceData = other.resourceData;
        }

        /// Refine resource (ví dụ: Ore -> Bar)
        public ResourceItem Refine(int quantity = 1)
        {
            if (!IsRefinable)
            {
                Debug.Log($"{Name} cannot be refined!");
                return null;
            }

            if (CurrentStack < quantity)
            {
                Debug.Log($"Not enough {Name} to refine! Need {quantity}, have {CurrentStack}");
                return null;
            }

            // Giảm resource gốc
            Stackable.RemoveFromStack(quantity);

            // Tạo refined result
            ResourceItem refined = new ResourceItem(RefinedResult);
            refined.Stackable.SetStack(RefineAmount * quantity);

            Debug.Log($"Refined {quantity} {Name} into {refined.CurrentStack} {refined.Name}");
            return refined;
        }

        /// Kiểm tra có thể craft item khác không
        public bool CanCraftWith(ResourceItem other)
        {
            // Logic crafting sẽ được implement trong CraftingSystem
            return true;
        }

        public override string ToString()
        {
            string refinable = IsRefinable ? " [Refinable]" : "";
            return $"{Name} x{CurrentStack} {refinable}";
        }
    }
}

