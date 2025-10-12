using UnityEngine;
using static UnityEditor.Progress;

namespace TinyFarm.Items
{
    // Tag để đánh dấu items(có thể có nhiều tags trên 1 item)
    // VD: "edible", "sellable", "craftable", "quest_item"...
    [System.Serializable]
    public class ItemTag
    {
        [Header("Identification")]
        [Tooltip("ID unique của tag")]
        public string tagID;

        [Tooltip("Tên hiển thị của tag")]
        public string tagName;

        [Header("Visual")]
        [Tooltip("Màu của tag (dùng cho UI)")]
        public Color tagColor = Color.gray;

        public ItemTag()
        {
            tagID = "";
            tagName = "New Tag";
            tagColor = Color.gray;
        }

        public ItemTag(string id, string name)
        {
            tagID = id;
            tagName = name;
            tagColor = Color.gray;
        }

        public ItemTag(string id, string name, Color color)
        {
            tagID = id;
            tagName = name;
            tagColor = color;
        }

        // So sánh 2 tag có giống nhau không (theo ID)
        public bool Equals(ItemTag other)
        {
            if (other == null) return false;
            return tagID == other.tagID;
        }

        // Override ToString để debug dễ hơn
        public override string ToString()
        {
            return $"[{tagID}] {tagName}";
        }

        // Các tag thường dùng
        public static class Common
        {
            // Functionality tags
            public static readonly string EDIBLE = "edible";               // Ăn được
            public static readonly string SELLABLE = "sellable";           // Bán được
            public static readonly string CRAFTABLE = "craftable";         // Chế tạo được
            public static readonly string PLANTABLE = "plantable";         // Trồng được

            // Category tags
            public static readonly string ORGANIC = "organic";             // Hữu cơ
            public static readonly string MINERAL = "mineral";             // Khoáng vật
            public static readonly string ANIMAL_PRODUCT = "animal_product"; // Sản phẩm động vật

            // Special tags
            public static readonly string SEASONAL = "seasonal";           // Theo mùa
            public static readonly string RARE = "rare";                   // Hiếm
        }

        // Tạo tag từ string ID nhanh
        public static ItemTag FromID(string tagID)
        {
            return new ItemTag(tagID, tagID);
        }
    }
}

