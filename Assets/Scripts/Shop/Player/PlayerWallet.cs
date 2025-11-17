using System;
using UnityEngine;

namespace TinyFarm
{
    // Quản lý gold/money của player
public class PlayerWallet : MonoBehaviour
{
    [Header("Starting Gold")]
        [SerializeField] private int startingGold = 500;
        
        [Header("Runtime")]
        [SerializeField] private int currentGold = 500;
        
        [Header("Debug")]
        [SerializeField] private bool debugMode = true;
        
        // Properties
        public int CurrentGold => currentGold;
        
        // Events
        public event Action<int> OnGoldChanged; // newAmount
        public event Action<int> OnGoldAdded;   // amount
        public event Action<int> OnGoldRemoved; // amount
        
        // Singleton (optional)
        public static PlayerWallet Instance { get; private set; }
        
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            currentGold = startingGold;
        }
        
        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }
        
        // ==========================================
        // PUBLIC API
        // ==========================================
        
        /// <summary>
        /// Add gold
        /// </summary>
        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            
            currentGold += amount;
            
            OnGoldAdded?.Invoke(amount);
            OnGoldChanged?.Invoke(currentGold);
        }
        
        /// <summary>
        /// Remove gold
        /// </summary>
        public bool RemoveGold(int amount)
        {
            if (amount <= 0) return false;
            
            if (currentGold < amount)
            {
                LogDebug($"Not enough gold! (Need: {amount}g, Have: {currentGold}g)");
                return false;
            }
            
            currentGold -= amount;
            
            OnGoldRemoved?.Invoke(amount);
            OnGoldChanged?.Invoke(currentGold);
            
            return true;
        }
        
        /// <summary>
        /// Check if player has enough gold
        /// </summary>
        public bool HasGold(int amount)
        {
            return currentGold >= amount;
        }
        
        /// <summary>
        /// Set gold directly (for save/load)
        /// </summary>
        public void SetGold(int amount)
        {
            currentGold = Mathf.Max(0, amount);
            OnGoldChanged?.Invoke(currentGold);
        }
        
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[Wallet] {message}");
            }
        }
    }
}

