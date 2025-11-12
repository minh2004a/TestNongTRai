using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.NPC
{
    // Entry cho mỗi item trong shop
    [System.Serializable]
    public class ShopItemEntry
    {
        [Header("Item")]
        public ItemData itemData;
        
        [Header("Stock")]
        public bool unlimitedStock = true;
        public int stock = 99;
        
        [Header("Availability")]
        [Tooltip("Để trống = available tất cả mùa")]
        public Season[] availableSeasons;
        
        [Header("Requirements (Optional)")]
        [Tooltip("Yêu cầu friendship level")]
        public int requiredFriendship = 0;
        
        [Tooltip("Yêu cầu đã unlock")]
        public bool requiresUnlock = false;
        public string unlockConditionID;

        // Runtime stock tracking
        [System.NonSerialized]
        public int currentStock;

        public ShopItemEntry()
        {
            unlimitedStock = true;
            stock = 99;
            currentStock = stock;
        }

        public bool IsAvailable()
        {
            if (requiresUnlock)
            {
                return false;
            }

            return true;
        }

        public bool HasStock()
        {
            return unlimitedStock || currentStock > 0;
        }

        public void RestoreStock()
        {
            currentStock = stock;
        }
        
        public bool TryPurchase(int quantity = 1)
        {
            if (unlimitedStock)
                return true;

            if (currentStock >= quantity)
            {
                currentStock -= quantity;
                return true;
            }
            
            return false;
        }
    }
}

