using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    [Serializable]
    public class EquipmentStats
    {
        [SerializeField] private int totalDefense;
        [SerializeField] private int totalAttack;
        [SerializeField] private int totalSpeed;
        [SerializeField] private Dictionary<string, int> customStats;
        [SerializeField] private List<string> activeBonuses;

        public int TotalDefense => totalDefense;
        public int TotalAttack => totalAttack;
        public int TotalSpeed => totalSpeed;
        public Dictionary<string, int> CustomStats => new Dictionary<string, int>(customStats);
        public List<string> ActiveBonuses => new List<string>(activeBonuses);

        public bool HasAnyStats => totalDefense > 0 || totalAttack > 0 || totalSpeed > 0 || customStats.Count > 0;

        public event Action<string, int> OnStatChanged;
        public event Action<string, bool> OnBonusChanged; // (bonusName, isActive)
        public event Action OnStatsRecalculated;

        public EquipmentStats()
        {
            totalDefense = 0;
            totalAttack = 0;
            totalSpeed = 0;
            customStats = new Dictionary<string, int>();
            activeBonuses = new List<string>();
        }

        /// Thêm stats từ equipment item
        public void AddStats(EquipmentItem equipment, EquipmentItemData equipmentItemData)
        {
            if (equipment == null) return;

            // Add base stats
            AddDefense(equipment.DefenseBonus);
            AddAttack(equipment.AttackBonus);
            AddSpeed(equipment.SpeedBonus);

            // Add custom stats
            var equipStats = equipmentItemData.GetAllStats();
            foreach (var stat in equipStats)
            {
                AddCustomStat(stat.Key, stat.Value);
            }

            // Add bonuses
            if (equipmentItemData.additionalStats != null && equipmentItemData.additionalStats.Count > 0)
            {
                foreach (var stat in equipmentItemData.additionalStats)
                {
                    AddBonus(stat.Key);
                }
            }

            OnStatsRecalculated?.Invoke();
        }

        /// Xóa stats từ equipment item
        public void RemoveStats(EquipmentItem equipment, EquipmentItemData equipmentItemData)
        {
            if (equipment == null) return;

            // Remove base stats
            RemoveDefense(equipment.DefenseBonus);
            RemoveAttack(equipment.AttackBonus);
            RemoveSpeed(equipment.SpeedBonus);

            // Remove custom stats
            var equipStats = equipmentItemData.GetAllStats();
            foreach (var stat in equipStats)
            {
                RemoveCustomStat(stat.Key, stat.Value);
            }

            // Remove bonuses
            if (equipmentItemData.additionalStats != null && equipmentItemData.additionalStats.Count > 0)
            {
                foreach (var stat in equipmentItemData.additionalStats)
                {
                    RemoveBonus(stat.Key);
                }
            }

            OnStatsRecalculated?.Invoke();
        }

        private void AddDefense(int amount)
        {
            if (amount <= 0) return;
            totalDefense += amount;
            OnStatChanged?.Invoke("defense", totalDefense);
        }

        private void RemoveDefense(int amount)
        {
            if (amount <= 0) return;
            totalDefense = Mathf.Max(0, totalDefense - amount);
            OnStatChanged?.Invoke("defense", totalDefense);
        }

        private void AddAttack(int amount)
        {
            if (amount <= 0) return;
            totalAttack += amount;
            OnStatChanged?.Invoke("attack", totalAttack);
        }

        private void RemoveAttack(int amount)
        {
            if (amount <= 0) return;
            totalAttack = Mathf.Max(0, totalAttack - amount);
            OnStatChanged?.Invoke("attack", totalAttack);
        }

        private void AddSpeed(int amount)
        {
            if (amount == 0) return;
            totalSpeed += amount;
            OnStatChanged?.Invoke("speed", totalSpeed);
        }

        private void RemoveSpeed(int amount)
        {
            if (amount == 0) return;
            totalSpeed -= amount;
            OnStatChanged?.Invoke("speed", totalSpeed);
        }

        private void AddCustomStat(string statName, int value)
        {
            if (string.IsNullOrEmpty(statName) || value == 0) return;

            if (customStats.ContainsKey(statName))
            {
                customStats[statName] += value;
            }
            else
            {
                customStats[statName] = value;
            }

            OnStatChanged?.Invoke(statName, customStats[statName]);
        }

        private void RemoveCustomStat(string statName, int value)
        {
            if (string.IsNullOrEmpty(statName) || !customStats.ContainsKey(statName)) return;

            customStats[statName] -= value;

            if (customStats[statName] <= 0)
            {
                customStats.Remove(statName);
            }

            OnStatChanged?.Invoke(statName, customStats.ContainsKey(statName) ? customStats[statName] : 0);
        }

        /// Lấy giá trị stat cụ thể
        public int GetStatValue(string statName)
        {
            switch (statName.ToLower())
            {
                case "defense": return totalDefense;
                case "attack": return totalAttack;
                case "speed": return totalSpeed;
                default:
                    return customStats.ContainsKey(statName) ? customStats[statName] : 0;
            }
        }

        private void AddBonus(string bonusName)
        {
            if (string.IsNullOrEmpty(bonusName)) return;

            if (!activeBonuses.Contains(bonusName))
            {
                activeBonuses.Add(bonusName);
                OnBonusChanged?.Invoke(bonusName, true);
            }
        }

        private void RemoveBonus(string bonusName)
        {
            if (string.IsNullOrEmpty(bonusName)) return;

            if (activeBonuses.Contains(bonusName))
            {
                activeBonuses.Remove(bonusName);
                OnBonusChanged?.Invoke(bonusName, false);
            }
        }

        /// Có bonus này đang active không?
        public bool HasBonus(string bonusName)
        {
            return activeBonuses.Contains(bonusName);
        }

        /// Reset tất cả stats về 0
        public void Clear()
        {
            totalDefense = 0;
            totalAttack = 0;
            totalSpeed = 0;
            customStats.Clear();
            activeBonuses.Clear();

            OnStatsRecalculated?.Invoke();
        }

        /// Lấy tất cả stats dưới dạng dictionary
        public Dictionary<string, int> GetAllStats()
        {
            Dictionary<string, int> allStats = new Dictionary<string, int>();

            if (totalDefense > 0) allStats["defense"] = totalDefense;
            if (totalAttack > 0) allStats["attack"] = totalAttack;
            if (totalSpeed != 0) allStats["speed"] = totalSpeed;

            foreach (var stat in customStats)
            {
                allStats[stat.Key] = stat.Value;
            }

            return allStats;
        }

        public override string ToString()
        {
            return $"[Stats] Def:{totalDefense} Atk:{totalAttack} Spd:{totalSpeed}";
        }
    }
}


