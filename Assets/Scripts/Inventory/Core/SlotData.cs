using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    [System.Serializable]
    public class SlotData
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private string itemID;
        [SerializeField] private int quantity;
        [SerializeField] private bool isLocked;
        [SerializeField] private SlotType slotType;

        // Custom data (cho durability, enchantments...)
        [SerializeField] private Dictionary<string, string> customData;

        public int SlotIndex => slotIndex;
        public string ItemID => itemID;
        public int Quantity => quantity;
        public bool IsLocked => isLocked;
        public SlotType SlotType => slotType;
        public Dictionary<string, string> CustomData => customData;

        public bool IsEmpty => string.IsNullOrEmpty(itemID) || quantity <= 0;

        public SlotData()
        {
            slotIndex = -1;
            itemID = string.Empty;
            quantity = 0;
            isLocked = false;
            slotType = SlotType.Normal;
            customData = new Dictionary<string, string>();
        }

        public SlotData(int index, SlotType type = SlotType.Normal)
        {
            slotIndex = index;
            itemID = string.Empty;
            quantity = 0;
            isLocked = false;
            slotType = type;
            customData = new Dictionary<string, string>();
        }

        // Tạo SlotData từ InventorySlot
        public static SlotData FromSlot(InventorySlot slot)
        {
            if (slot == null)
            {
                Debug.LogError("[SlotData] Cannot create from null slot!");
                return null;
            }

            SlotData data = new SlotData
            {
                slotIndex = slot.SlotIndex,
                slotType = slot.SlotType,
                isLocked = slot.IsLocked,
                customData = new Dictionary<string, string>()
            };

            // Lưu item data nếu có
            if (!slot.IsEmpty)
            {
                data.itemID = slot.ItemID;
                data.quantity = slot.Quantity;

                // Save custom data từ item (nếu có)
                // VD: durability, enchantments...
                // TODO: Implement custom data serialization nếu cần
            }

            return data;
        }

        // Restore InventorySlot từ SlotData
        public InventorySlot ToSlot(ItemDatabase itemDatabase)
        {
            if (itemDatabase == null)
            {
                Debug.LogError("[SlotData] ItemDatabase is null!");
                return null;
            }

            // Tạo slot mới
            InventorySlot slot = new InventorySlot(slotIndex, slotType);

            // Restore lock state
            if (isLocked)
            {
                slot.Lock();
            }

            // Restore item nếu có
            if (!IsEmpty)
            {
                ItemData itemData = itemDatabase.GetItem<ItemData>(itemID);
                if (itemData != null)
                {
                    // Tạo item từ data
                    Item item = CreateItemFromData(itemData);

                    if (item != null)
                    {
                        // Set quantity
                        if (item.IsStackable)
                        {
                            item.Stackable.SetStack(quantity);
                        }

                        // Add vào slot
                        slot.SetItem(item);

                        // Restore custom data nếu có
                        // TODO: Implement custom data restoration
                    }
                }
                else
                {
                    Debug.LogWarning($"[SlotData] ItemData '{itemID}' not found in database!");
                }
            }

            return slot;
        }

        private Item CreateItemFromData(ItemData itemData)
        {
            // Tạo item instance dựa trên type
            switch (itemData.GetItemType())
            {
                case ItemType.Seed:
                    if (itemData is SeedItemData seedData)
                        return new SeedItem(seedData);
                    break;

                case ItemType.Crop:
                    if (itemData is CropItemData cropData)
                        return new CropItem(cropData);
                    break;

                case ItemType.Tool:
                    if (itemData is ToolItemData toolData)
                        return new ToolItem(toolData);
                    break;

                case ItemType.Resources:
                    if (itemData is ResourcesItemData resourceData)
                        return new ResourceItem(resourceData);
                    break;

                case ItemType.Equipment:
                    if (itemData is EquipmentItemData equipData)
                        return new EquipmentItem(equipData);
                    break;

                case ItemType.CraftingMaterial:
                    if (itemData is CraftingMaterialData matData)
                        return new CraftingMaterial(matData);
                    break;

                default:
                    // Generic item (nếu có base Item constructor)
                    Debug.LogWarning($"[SlotData] Unknown item type: {itemData.GetItemType()}");
                    break;
            }

            return null;
        }

        public void SetCustomData(string key, string value)
        {
            if (customData == null)
            {
                customData = new Dictionary<string, string>();
            }

            customData[key] = value;
        }

        public string GetCustomData(string key, string defaultValue = "")
        {
            if (customData != null && customData.ContainsKey(key))
            {
                return customData[key];
            }

            return defaultValue;
        }

        public bool HasCustomData(string key)
        {
            return customData != null && customData.ContainsKey(key);
        }

        public override string ToString()
        {
            if (IsEmpty)
            {
                return $"[SlotData {slotIndex}] Empty";
            }

            return $"[SlotData {slotIndex}] {itemID} x{quantity}";
        }

        /// Clone SlotData
        public SlotData Clone()
        {
            SlotData clone = new SlotData
            {
                slotIndex = slotIndex,
                itemID = itemID,
                quantity = quantity,
                isLocked = isLocked,
                slotType = slotType,
                customData = new Dictionary<string, string>(customData)
            };

            return clone;
        }

        /// Validate data
        public bool Validate()
        {
            if (slotIndex < 0)
            {
                Debug.LogWarning("[SlotData] Invalid slot index!");
                return false;
            }

            if (!IsEmpty && quantity <= 0)
            {
                Debug.LogWarning("[SlotData] Invalid quantity!");
                return false;
            }

            return true;
        }
    }

}
