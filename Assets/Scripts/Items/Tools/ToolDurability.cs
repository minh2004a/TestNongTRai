//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace TinyFarm.Items
//{
//    [Serializable]
//    public class ToolDurability
//    {
//        private ToolItem tool;

//        // Durability thresholds
//        [Header("Durability Thresholds")]
//        [Tooltip("Efficiency penalty khi dưới threshold")]
//        [SerializeField] private float lowDurabilityThreshold = 0.25f; // 25%

//        [Tooltip("Efficiency ở low durability (0.5 = 50% efficiency)")]
//        [SerializeField] private float lowDurabilityEfficiency = 0.5f;

//        [Tooltip("Có thể sử dụng khi durability = 0?")]
//        [SerializeField] private bool canUseWhenBroken = false;

//        // Repair settings
//        //[Header("Repair Settings")]
//        //[Tooltip("Vật liệu cần để repair")]
//        //[SerializeField] private List<RepairMaterial> repairMaterials = new List<RepairMaterial>();

//        [Tooltip("Gold cost để repair 1 durability point")]
//        [SerializeField] private int goldCostPerPoint = 1;

//        // Events
//        public event Action<float> OnDurabilityChanged;
//        public event Action OnToolBroken;
//        public event Action OnToolRepaired;
//        public event Action<float> OnLowDurability;

//        // Properties
//        public ToolItem Tool => tool;
//        public float DurabilityPercent => tool?.Durability.DurabilityPercent ?? 0f;
//        public bool IsBroken => tool?.IsBroken ?? false;
//        public bool IsLowDurability => DurabilityPercent < lowDurabilityThreshold * 100f;
//        public bool CanUse => canUseWhenBroken || !IsBroken;

//        public ToolDurability(ToolItem tool)
//        {
//            this.tool = tool;

//            // Subscribe to durability events
//            if (tool?.Durability != null)
//            {
//                tool.Durability.OnDurabilityChanged += OnDurabilityChangedHandler;
//                tool.Durability.OnItemBroken += OnItemBrokenHandler;
//            }
//        }

//        // Use tool (giảm durability)
//        public bool UseTool()
//        {
//            if (!CanUse)
//            {
//                Debug.Log($"Cannot use {tool.Name} - tool is broken!");
//                return false;
//            }

//            // Giảm durability
//            //float loss = CalculateDurabilityLoss();
//            //tool.Durability.Damage(loss);

//            return true;
//        }

//        // Kiểm tra có thể repair không
//        //public bool CanRepair(InventoryManager inventory)
//        //{
//        //    if (!tool.HasDurability) return false;
//        //    if (tool.Durability.CurrentDurability >= tool.Durability.MaxDurability) return false;

//        //    // Check materials
//        //    foreach (var material in repairMaterials)
//        //    {
//        //        if (!inventory.HasItem(material.itemID, material.quantity))
//        //        {
//        //            return false;
//        //        }
//        //    }

//        //    return true;
//        //}

//        // Repair tool với materials
//        //public bool Repair(InventoryManager inventory, float repairAmount)
//        //{
//        //    if (!CanRepair(inventory))
//        //    {
//        //        Debug.Log("Cannot repair - missing materials or tool is full");
//        //        return false;
//        //    }

//        //    // Remove materials
//        //    foreach (var material in repairMaterials)
//        //    {
//        //        inventory.RemoveItem(material.itemID, material.quantity);
//        //    }

//        //    // Repair
//        //    tool.Durability.Repair(repairAmount);

//        //    OnToolRepaired?.Invoke();
//        //    return true;
//        //}

//        // Repair toàn bộ
//        //public bool RepairFull(InventoryManager inventory)
//        //{
//        //    float missingDurability = tool.Durability.MaxDurability - tool.Durability.CurrentDurability;
//        //    return Repair(inventory, missingDurability);
//        //}

//        /// Tính cost để repair
//        public int CalculateRepairCost()
//        {
//            float missingDurability = tool.Durability.MaxDurability - tool.Durability.CurrentDurability;
//            return Mathf.CeilToInt(missingDurability * goldCostPerPoint * tool.ItemData.repairCostMultiplier);
//        }

//        ///// Get required materials để repair
//        //public List<RepairMaterial> GetRequiredMaterials()
//        //{
//        //    return new List<RepairMaterial>(repairMaterials);
//        //}

//        private void OnDurabilityChangedHandler(float newDurability)
//        {
//            OnDurabilityChanged?.Invoke(newDurability);

//            // Check low durability
//            if (IsLowDurability)
//            {
//                OnLowDurability?.Invoke(DurabilityPercent);
//            }
//        }

//        private void OnItemBrokenHandler()
//        {
//            OnToolBroken?.Invoke();
//            Debug.Log($"{tool.Name} is broken!");
//        }

//        // ===== UTILITY =====

//        public string GetStatusText()
//        {
//            if (IsBroken) return "BROKEN";
//            if (IsLowDurability) return $"LOW ({DurabilityPercent:F0}%)";
//            return $"{DurabilityPercent:F0}%";
//        }


//    }
//}

