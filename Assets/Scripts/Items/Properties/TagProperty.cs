using System;
using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.Items
{
    [Serializable]
    public class TagProperty
    {
        [SerializeField] private List<ItemTag> tags = new List<ItemTag>();

        // Properties
        public IReadOnlyList<ItemTag> Tags => tags;
        public int TagCount => tags.Count;
        public bool HasAnyTags => tags.Count > 0;

        // Constructor
        public TagProperty()
        {
            tags = new List<ItemTag>();
        }

        public TagProperty(params ItemTag[] initialTags)
        {
            tags = new List<ItemTag>();
            AddTags(initialTags);
        }

        // Thêm tag
        public bool AddTag(ItemTag tag)
        {
            if (tag == null || tags.Contains(tag))
                return false;

            tags.Add(tag);
            return true;
        }

        // Thêm nhiều tags
        public int AddTags(params ItemTag[] tagsToAdd)
        {
            int added = 0;
            foreach (var tag in tagsToAdd)
            {
                if (AddTag(tag))
                    added++;
            }
            return added;
        }

        // Xóa tag
        public bool RemoveTag(ItemTag tag)
        {
            return tags.Remove(tag);
        }

        // Xóa nhiều tags
        public int RemoveTags(params ItemTag[] tagsToRemove)
        {
            int removed = 0;
            foreach (var tag in tagsToRemove)
            {
                if (RemoveTag(tag))
                    removed++;
            }
            return removed;
        }

        // Kiểm tra có tag
        public bool HasTag(ItemTag tag)
        {
            return tag != null && tags.Contains(tag);
        }

        // Kiểm tra có bất kỳ tag nào trong danh sách
        public bool HasAnyTag(params ItemTag[] tagsToCheck)
        {
            foreach (var tag in tagsToCheck)
            {
                if (HasTag(tag))
                    return true;
            }
            return false;
        }

        // Kiểm tra có tất cả tags trong danh sách
        public bool HasAllTags(params ItemTag[] tagsToCheck)
        {
            foreach (var tag in tagsToCheck)
            {
                if (!HasTag(tag))
                    return false;
            }
            return true;
        }

        // Lấy tags theo tên
        public ItemTag GetTagByName(string tagName)
        {
            return tags.Find(t => t != null && t.tagName == tagName);
        }

        // Filter tags theo điều kiện
        public List<ItemTag> GetTagsWhere(Func<ItemTag, bool> predicate)
        {
            var result = new List<ItemTag>();
            foreach (var tag in tags)
            {
                if (tag != null && predicate(tag))
                    result.Add(tag);
            }
            return result;
        }

        // Clear tất cả tags
        public void ClearTags()
        {
            tags.Clear();
        }

        // Toggle tag (thêm nếu chưa có, xóa nếu đã có)
        public bool ToggleTag(ItemTag tag)
        {
            if (HasTag(tag))
            {
                RemoveTag(tag);
                return false;
            }
            else
            {
                AddTag(tag);
                return true;
            }
        }

        // Clone
        public TagProperty Clone()
        {
            var clone = new TagProperty();
            foreach (var tag in tags)
            {
                clone.AddTag(tag);
            }
            return clone;
        }

        // So sánh tags với TagProperty khác
        public bool HasSameTagsAs(TagProperty other)
        {
            if (other == null || TagCount != other.TagCount)
                return false;

            foreach (var tag in tags)
            {
                if (!other.HasTag(tag))
                    return false;
            }
            return true;
        }
    }
}


