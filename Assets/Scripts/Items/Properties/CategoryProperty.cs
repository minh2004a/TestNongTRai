using System;
using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.Items
{
    [Serializable]
    public class CategoryProperty
    {
        [SerializeField] private ItemCategory primaryCategory;
        [SerializeField] private List<ItemCategory> secondaryCategories = new List<ItemCategory>();

        // Properties
        public ItemCategory PrimaryCategory => primaryCategory;
        public IReadOnlyList<ItemCategory> SecondaryCategories => secondaryCategories;
        public int TotalCategories => 1 + secondaryCategories.Count;

        // Constructor
        public CategoryProperty(ItemCategory primaryCategory)
        {
            this.primaryCategory = primaryCategory;
        }

        // Thêm category phụ
        public bool AddSecondaryCategory(ItemCategory category)
        {
            if (category == null || secondaryCategories.Contains(category))
                return false;

            if (category == primaryCategory)
                return false;

            secondaryCategories.Add(category);
            return true;
        }

        // Xóa category phụ
        public bool RemoveSecondaryCategory(ItemCategory category)
        {
            return secondaryCategories.Remove(category);
        }

        // Kiểm tra có category không
        public bool HasCategory(ItemCategory category)
        {
            if (category == null) return false;
            if (primaryCategory == category) return true;
            return secondaryCategories.Contains(category);
        }

        // Kiểm tra có bất kỳ category nào trong danh sách
        public bool HasAnyCategory(params ItemCategory[] categories)
        {
            foreach (var category in categories)
            {
                if (HasCategory(category))
                    return true;
            }
            return false;
        }

        // Kiểm tra có tất cả category trong danh sách
        public bool HasAllCategories(params ItemCategory[] categories)
        {
            foreach (var category in categories)
            {
                if (!HasCategory(category))
                    return false;
            }
            return true;
        }

        // Lấy tất cả categories
        public List<ItemCategory> GetAllCategories()
        {
            var allCategories = new List<ItemCategory> { primaryCategory };
            allCategories.AddRange(secondaryCategories);
            return allCategories;
        }

        // Set primary category
        public void SetPrimaryCategory(ItemCategory category)
        {
            if (category == null) return;

            // Nếu category mới đang ở secondary, xóa nó khỏi secondary
            secondaryCategories.Remove(category);
            primaryCategory = category;
        }

        // Clear tất cả secondary categories
        public void ClearSecondaryCategories()
        {
            secondaryCategories.Clear();
        }

        // Clone
        public CategoryProperty Clone()
        {
            var clone = new CategoryProperty(primaryCategory);
            foreach (var category in secondaryCategories)
            {
                clone.AddSecondaryCategory(category);
            }
            return clone;
        }
    }
}
