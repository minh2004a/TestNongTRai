using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    // Quản lý tất cả equipment của player
    // Xử lý equip/unequip, tính stats, integration với inventory
    public class EquipmentManager : MonoBehaviour
    {
        [Header("Equipment Slots")]
        [SerializeField] private Dictionary<EquipmentSlotType, EquipmentSlot> equipmentSlots;

        [Header("Stats")]
        [SerializeField] private EquipmentStats equipmentStats;
        [Header("Data")]
        [SerializeField] private EquipmentItemData equipmentItemData;

        [Header("References")]
        [SerializeField] private InventoryManager inventoryManager;

        [Header("Settings")]
        [SerializeField] private bool autoCalculateStats = true;
        [SerializeField] private int playerLevel = 1;

        public EquipmentItemData EquipmentItemData => equipmentItemData;
        public int PlayerLevel
        {
            get => playerLevel;
            set => playerLevel = value;
        }

        public event Action<EquipmentSlotType, EquipmentItem> OnItemEquipped;
        public event Action<EquipmentSlotType, EquipmentItem> OnItemUnequipped;
        public event Action OnEquipmentChanged;
        public event Action OnStatsUpdated;

        private void Awake()
        {
            InitializeSlots();
            EquipmentItemData data = ScriptableObject.CreateInstance<EquipmentItemData>();
        }

        private void Start()
        {
            if (inventoryManager == null)
            {
                inventoryManager = FindObjectOfType<InventoryManager>();
            }
        }
        private void InitializeSlots()
        {
            equipmentSlots = new Dictionary<EquipmentSlotType, EquipmentSlot>();

            foreach (EquipmentSlotType slotType in System.Enum.GetValues(typeof(EquipmentSlotType)))
            {
                EquipmentSlot slot = new EquipmentSlot(slotType);

                // Subscribe to slot events
                slot.OnItemEquipped += (item) => HandleItemEquipped(slotType, item);
                slot.OnItemUnequipped += (item) => HandleItemUnequipped(slotType, item);

                equipmentSlots[slotType] = slot;
            }

            Debug.Log($"[EquipmentManager] Initialized {equipmentSlots.Count} equipment slots");
        }

        private void HandleItemEquipped(EquipmentSlotType slotType, EquipmentItem item)
        {
            if (autoCalculateStats)
            {
                equipmentStats.AddStats(item, equipmentItemData);
            }

            OnItemEquipped?.Invoke(slotType, item);
            OnEquipmentChanged?.Invoke();
            OnStatsUpdated?.Invoke();

            Debug.Log($"[EquipmentManager] Equipped {item.GetDisplayName()} in {slotType} slot");
        }

        private void HandleItemUnequipped(EquipmentSlotType slotType, EquipmentItem item)
        {
            if (autoCalculateStats)
            {
                equipmentStats.RemoveStats(item, equipmentItemData);
            }

            OnItemUnequipped?.Invoke(slotType, item);
            OnEquipmentChanged?.Invoke();
            OnStatsUpdated?.Invoke();

            Debug.Log($"[EquipmentManager] Unequipped {item.GetDisplayName()} from {slotType} slot");
        }

        // Trang bị item từ inventory
        public bool EquipFromInventory(EquipmentItem item)
        {
            if (item == null)
            {
                Debug.LogError("[EquipmentManager] Cannot equip null item!");
                return false;
            }

            EquipmentSlotType slotType = item.SlotType;

            if (!equipmentSlots.ContainsKey(slotType))
            {
                Debug.LogError($"[EquipmentManager] Slot type {slotType} not found!");
                return false;
            }

            EquipmentSlot slot = equipmentSlots[slotType];

            // Nếu slot đã có item, unequip và trả về inventory
            if (slot.IsOccupied)
            {
                EquipmentItem oldItem = slot.Unequip();
                if (inventoryManager != null && oldItem != null)
                {
                    inventoryManager.AddItem(oldItem);
                }
            }

            // Equip item mới
            bool success = slot.Equip(item);

            if (success && inventoryManager != null)
            {
                // Remove from inventory
                inventoryManager.RemoveItem(item.ItemData, 1);
            }

            return success;
        }
        // Tháo item và trả về inventory
        public bool UnequipToInventory(EquipmentSlotType slotType)
        {
            if (!equipmentSlots.ContainsKey(slotType))
            {
                Debug.LogError($"[EquipmentManager] Slot type {slotType} not found!");
                return false;
            }

            EquipmentSlot slot = equipmentSlots[slotType];

            if (slot.IsEmpty)
            {
                Debug.LogWarning($"[EquipmentManager] Slot {slotType} is already empty!");
                return false;
            }

            EquipmentItem unequippedItem = slot.Unequip();

            if (unequippedItem != null && inventoryManager != null)
            {
                // Add back to inventory
                bool added = inventoryManager.AddItem(unequippedItem);
                if (!added)
                {
                    Debug.LogError("[EquipmentManager] Failed to add unequipped item to inventory!");
                    // Re-equip if inventory full
                    slot.Equip(unequippedItem);
                    return false;
                }
            }

            return true;
        }

        // Tháo tất cả equipment
        public void UnequipAll()
        {
            foreach (var slotType in equipmentSlots.Keys)
            {
                UnequipToInventory(slotType);
            }

            Debug.Log("[EquipmentManager] Unequipped all items");
        }

        // Lấy item đang equip ở slot cụ thể
        public EquipmentItem GetEquippedItem(EquipmentSlotType slotType)
        {
            if (equipmentSlots.ContainsKey(slotType))
            {
                return equipmentSlots[slotType].EquippedItem;
            }
            return null;
        }

        /// Lấy slot cụ thể
        public EquipmentSlot GetSlot(EquipmentSlotType slotType)
        {
            return equipmentSlots.ContainsKey(slotType) ? equipmentSlots[slotType] : null;
        }

        /// Slot có đang bị chiếm không?
        public bool IsSlotOccupied(EquipmentSlotType slotType)
        {
            return equipmentSlots.ContainsKey(slotType) && equipmentSlots[slotType].IsOccupied;
        }

        /// Lấy tất cả equipped items
        public List<EquipmentItem> GetAllEquippedItems()
        {
            List<EquipmentItem> items = new List<EquipmentItem>();

            foreach (var slot in equipmentSlots.Values)
            {
                if (slot.IsOccupied)
                {
                    items.Add(slot.EquippedItem);
                }
            }

            return items;
        }

        /// Đếm số equipment đang đeo
        public int GetEquippedCount()
        {
            int count = 0;
            foreach (var slot in equipmentSlots.Values)
            {
                if (slot.IsOccupied) count++;
            }
            return count;
        }

        /// Lấy tổng stats từ tất cả equipment
        public Dictionary<string, int> GetTotalStats()
        {
            return equipmentStats.GetAllStats();
        }

        /// Lấy giá trị stat cụ thể
        public int GetStatValue(string statName)
        {
            return equipmentStats.GetStatValue(statName);
        }

        /// Có bonus này đang active không?
        public bool HasBonus(string bonusName)
        {
            return equipmentStats.HasBonus(bonusName);
        }

        /// Recalculate tất cả stats (manual)
        public void RecalculateStats(EquipmentItem item, EquipmentItemData equipmentItemData)
        {
            equipmentStats.Clear();

            foreach (var slot in equipmentSlots.Values)
            {
                if (slot.IsOccupied)
                {
                    equipmentStats.AddStats(slot.EquippedItem, (EquipmentItemData)slot.EquippedItem.ItemData);
                }
            }

            OnStatsUpdated?.Invoke();
            Debug.Log("[EquipmentManager] Stats recalculated");
        }

        /// Save equipment state
        public EquipmentSaveData SaveEquipment()
        {
            EquipmentSaveData saveData = new EquipmentSaveData();

            foreach (var kvp in equipmentSlots)
            {
                if (kvp.Value.IsOccupied)
                {
                    saveData.equippedItems[kvp.Key] = kvp.Value.EquippedItem.InstanceID;
                }
            }

            return saveData;
        }

        /// <summary>
        /// Load equipment state
        /// </summary>
        public void LoadEquipment(EquipmentSaveData saveData, ItemManager itemManager)
        {
            if (saveData == null || itemManager == null) return;

            UnequipAll();

            foreach (var kvp in saveData.equippedItems)
            {
                EquipmentSlotType slotType = kvp.Key;
                string instanceID = kvp.Value;

                // Get item from ItemManager by instanceID
                // (ItemManager cần có method GetItemByInstanceID)
                // Item item = itemManager.GetItemByInstanceID(instanceID);

                // if (item is EquipmentItem equipItem)
                // {
                //     EquipFromInventory(equipItem);
                // }
            }

            Debug.Log("[EquipmentManager] Equipment loaded");
        }


        [ContextMenu("Debug Equipment Status")]
        private void DebugEquipmentStatus()
        {
            Debug.Log("=== EQUIPMENT STATUS ===");

            foreach (var kvp in equipmentSlots)
            {
                string status = kvp.Value.IsOccupied
                    ? kvp.Value.EquippedItem.GetDisplayName()
                    : "Empty";
                Debug.Log($"{kvp.Key}: {status}");
            }

            Debug.Log($"\nTotal Stats:\n{equipmentStats}");
        }
    }

    [System.Serializable]
    public class EquipmentSaveData
    {
        public Dictionary<EquipmentSlotType, string> equippedItems = new Dictionary<EquipmentSlotType, string>();
    }
}
