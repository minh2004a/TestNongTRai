using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    [System.Serializable]
    public class InventoryData
    {
        [SerializeField] private List<SlotData> slots;
        [SerializeField] private int inventorySize;
        [SerializeField] private string inventoryName;
        [SerializeField] private long saveTimestamp;


        public List<SlotData> Slots => slots;
        public int InventorySize => inventorySize;
        public string InventoryName => inventoryName;
        public long SaveTimestamp => saveTimestamp;

        public DateTime SaveTime => new DateTime(saveTimestamp);

        public InventoryData()
        {
            slots = new List<SlotData>();
            inventorySize = 0;
            inventoryName = "Inventory";
            saveTimestamp = DateTime.Now.Ticks;
        }

        public InventoryData(string name, int size)
        {
            slots = new List<SlotData>();
            inventorySize = size;
            inventoryName = name;
            saveTimestamp = DateTime.Now.Ticks;
        }

        // Thêm SlotData vào inventory data
        public void AddSlot(SlotData slotData)
        {
            if (slotData == null) return;
            slots.Add(slotData);
        }

        // Lấy SlotData theo index
        public SlotData GetSlot(int index)
        {
            if (index < 0 || index >= slots.Count) return null;
            return slots[index];
        }

        // Clear tất cả slots
        public void Clear()
        {
            slots.Clear();
        }

        // Update save timestamp
        public void UpdateTimestamp()
        {
            saveTimestamp = DateTime.Now.Ticks;
        }

        // Validate data
        public bool Validate()
        {
            if (inventorySize <= 0)
            {
                Debug.LogWarning("[InventoryData] Invalid inventory size!");
                return false;
            }

            foreach (var slot in slots)
            {
                if (!slot.Validate())
                {
                    Debug.LogWarning("[InventoryData] Invalid slot data!");
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            int occupiedSlots = 0;
            foreach (var slot in slots)
            {
                if (!slot.IsEmpty) occupiedSlots++;
            }

            return $"[InventoryData] {inventoryName} - {occupiedSlots}/{inventorySize} slots occupied";
        }
    }
}

