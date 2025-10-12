using UnityEngine;

namespace TinyFarm.Items
{
    // Category để phân loại items(có thể có nhiều level)
    /// VD: "Vegetables", "Fruits", "Ores"...
    [System.Serializable]
    public class ItemCategory
    {
        [Header("Identification")]
        [Tooltip("ID unique của category")]
        public string categoryID;

        [Tooltip("Tên hiển thị của category")]
        public string categoryName;

        [Header("Visual")]
        [Tooltip("Màu đại diện cho category")]
        public Color categoryColor = Color.white;

        [Tooltip("Icon của category")]
        public Sprite categoryIcon;

        [Header("Settings")]
        [Tooltip("Thứ tự sắp xếp (số nhỏ lên trước)")]
        [Range(0, 100)]
        public int sortOrder = 0;

        public ItemCategory()
        {
            categoryID = "";
            categoryName = "New Category";
            categoryColor = Color.white;
            sortOrder = 0;
        }

        public ItemCategory(string id, string name)
        {
            categoryID = id;
            categoryName = name;
            categoryColor = Color.white;
            sortOrder = 0;
        }

        public ItemCategory(string id, string name, Color color)
        {
            categoryID = id;
            categoryName = name;
            categoryColor = color;
            sortOrder = 0;
        }

        // So sánh 2 category có giống nhau không (theo ID)
        public bool Equals(ItemCategory other)
        {
            if (other == null) return false;
            return categoryID == other.categoryID;
        }

        // Override ToString để debug dễ hơn
        public override string ToString()
        {
            return $"[{categoryID}] {categoryName}";
        }

        // Các category mặc định có thể dùng
        public static class Predefined
        {
            public static ItemCategory Vegetables = new ItemCategory("vegetables", "Vegetables", new Color(0.2f, 0.8f, 0.2f));
            public static ItemCategory Fruits = new ItemCategory("fruits", "Fruits", new Color(1f, 0.4f, 0.4f));
            public static ItemCategory Grains = new ItemCategory("grains", "Grains", new Color(0.9f, 0.8f, 0.4f));
            public static ItemCategory Gems = new ItemCategory("gems", "Gems", new Color(0.4f, 0.8f, 1f));
            public static ItemCategory Weapons = new ItemCategory("weapons", "Weapons", new Color(0.8f, 0.2f, 0.2f));
            public static ItemCategory Armor = new ItemCategory("armor", "Armor", new Color(0.6f, 0.6f, 0.8f));
        }
    }

}
