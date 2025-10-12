using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    [Serializable]
    public class DurabilityProperty
    {
        [SerializeField] private bool hasDurability = true;
        [SerializeField] private float maxDurability = 100f;
        [SerializeField] private float currentDurability = 100f;
        [SerializeField] private bool destroyOnBreak = true;
        [SerializeField] private float repairCostMultiplier = 1f;

        // Properties
        public bool HasDurability => hasDurability;
        public float MaxDurability => maxDurability;
        public float CurrentDurability => currentDurability;
        public float DurabilityPercent => maxDurability > 0? (currentDurability / maxDurability) * 100f : 0f;
        public bool IsBroken => currentDurability <= 0f;
        public bool DestroyOnBreak => destroyOnBreak;
        public float RepairCostMultiplier => repairCostMultiplier;

        // Events
        public event Action<float> OnDurabilityChanged;
        public event Action OnItemBroken;
        public event Action OnItemRepaired;

        // Constructor
        public DurabilityProperty(bool hasDurability = true, float maxDurability = 100f, 
            bool destroyOnBreak = true, float repairCostMultiplier = 1f)
        {
            this.hasDurability = hasDurability;
            this.maxDurability = Mathf.Max(1f, maxDurability);
            this.currentDurability = this.maxDurability;
            this.destroyOnBreak = destroyOnBreak;
            this.repairCostMultiplier = Mathf.Max(0f, repairCostMultiplier);
        }

        // Giảm độ bền
        public void Damage(float amount)
        {
            if (!hasDurability || amount <= 0f) return;

            float oldDurability = currentDurability;
            currentDurability = Mathf.Max(0f, currentDurability - amount);

            OnDurabilityChanged?.Invoke(currentDurability);

            if (!IsBroken && oldDurability > 0f && currentDurability <= 0f)
            {
                OnItemBroken?.Invoke();
            }
        }

        // Sửa chữa
        public void Repair(float amount)
        {
            if (!hasDurability || amount <= 0f) return;

            bool wasBroken = IsBroken;
            float oldDurability = currentDurability;

            currentDurability = Mathf.Min(maxDurability, currentDurability + amount);

            OnDurabilityChanged?.Invoke(currentDurability);

            if (wasBroken && currentDurability > 0f)
            {
                OnItemRepaired?.Invoke();
            }
        }

        // Sửa chữa hoàn toàn
        public void RepairFully()
        {
            Repair(maxDurability);
        }

        // Set độ bền trực tiếp
        public void SetDurability(float value)
        {
            if (!hasDurability) return;

            bool wasBroken = IsBroken;
            float oldDurability = currentDurability;

            currentDurability = Mathf.Clamp(value, 0f, maxDurability);

            OnDurabilityChanged?.Invoke(currentDurability);

            if (!wasBroken && currentDurability <= 0f)
            {
                OnItemBroken?.Invoke();
            }
            else if (wasBroken && currentDurability > 0f)
            {
                OnItemRepaired?.Invoke();
            }
        }

        // Tính chi phí sửa chữa
        public float GetRepairCost()
        {
            if (!hasDurability || currentDurability >= maxDurability) return 0f;

            float damageProportion = (maxDurability - currentDurability) / maxDurability;
            return damageProportion * repairCostMultiplier;
        }

        // Kiểm tra có thể sử dụng không (dựa vào độ bền)
        public bool CanUse()
        {
            return !hasDurability || currentDurability > 0f;
        }

        // Clone
        public DurabilityProperty Clone()
        {
            var clone = new DurabilityProperty(hasDurability, maxDurability, destroyOnBreak, repairCostMultiplier);
            clone.currentDurability = this.currentDurability;
            return clone;
        }
    }
}

