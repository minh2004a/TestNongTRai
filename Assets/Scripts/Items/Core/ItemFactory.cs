using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TinyFarm.Items.ItemValidator;


namespace TinyFarm.Items
{
    public class ItemFactory
    {
        private ItemDatabase database;
        private bool validateOnCreate = true;

        public ItemFactory(ItemDatabase database, bool validateOnCreate = true)
        {
            this.database = database;
            this.validateOnCreate = validateOnCreate;
        }

        // Tạo item từ ItemData
        public Item CreateItem(ItemData itemData)
        {
            if (itemData == null)
            {
                Debug.LogError("Cannot create item from null ItemData");
                return null;
            }

            // Validate nếu cần
            if (validateOnCreate)
            {
                var validation = ItemValidator.ValidationResult.ValidateItemData(itemData);
                if (!validation.IsValid)
                {
                    Debug.LogError($"Invalid ItemData: {validation.GetReport()}");
                    return null;
                }
            }

            // Tạo item dựa trên type
            return CreateItemByType(itemData);
        }

        // Tạo item từ ID
        public Item CreateItem(string itemID)
        {
            if (database == null)
            {
                Debug.LogError("ItemFactory has no database");
                return null;
            }

            ItemData itemData = database.GetItemByID(itemID);
            if (itemData == null)
            {
                Debug.LogError($"Item with ID '{itemID}' not found in database");
                return null;
            }

            return CreateItem(itemData);
        }

        // Tạo item với số lượng custom
        public Item CreateItem(string itemID, int quantity)
        {
            Item item = CreateItem(itemID);
            if (item != null && item.IsStackable)
            {
                item.Stackable.SetStack(quantity);
            }
            return item;
        }

        // Tạo item với durability custom
        public Item CreateItem(string itemID, int quantity, float durability)
        {
            Item item = CreateItem(itemID, quantity);
            if (item != null && item.HasDurability)
            {
                item.Durability.SetDurability(durability);
            }
            return item;
        }

        // Tạo nhiều items cùng lúc
        public List<Item> CreateItems(Dictionary<string, int> itemQuantities)
        {
            var items = new List<Item>();

            foreach (var kvp in itemQuantities)
            {
                Item item = CreateItem(kvp.Key, kvp.Value);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }

        // Clone item
        public Item CloneItem(Item original)
        {
            if (original == null)
            {
                Debug.LogError("Cannot clone null item");
                return null;
            }

            return original.Clone();
        }

        // Tạo item dựa trên type cụ thể
        private Item CreateItemByType(ItemData itemData)
        {
            // Check specialized types
            if (itemData is ToolItemData toolData)
                return new ToolItem(toolData);

            if (itemData is SeedItemData seedData)
                return new SeedItem(seedData);

            if (itemData is CropItemData cropData)
                return new CropItem(cropData);

            if (itemData is ResourcesItemData resourceData)
                return new ResourceItem(resourceData);

            if (itemData is EquipmentItemData equipmentData)
                return new EquipmentItem(equipmentData);

            if (itemData is CraftingMaterialData materialData)
                return new CraftingMaterial(materialData);

            // Default: tạo Item thông thường
            return new Item(itemData);
        }

        /// Batch create items (tối ưu performance)
        public List<Item> BatchCreateItems(List<string> itemIDs)
        {
            var items = new List<Item>();

            foreach (var id in itemIDs)
            {
                Item item = CreateItem(id);
                if (item != null)
                {
                    items.Add(item);
                }
            }

            return items;
        }
    }
}

