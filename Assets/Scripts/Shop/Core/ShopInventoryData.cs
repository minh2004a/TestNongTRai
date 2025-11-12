using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.NPC
{
    // Data cho shop inventory - items bán theo mùa
    [CreateAssetMenu(fileName = "ShopInventory", menuName = "Game/Shop/Shop Inventory")]
    public class ShopInventoryData : ScriptableObject
    {
        [Header("Shop Info")]
        [SerializeField] private string shopName = "Pierre's General Store";

        [Header("Shop Items")]
        [SerializeField] private List<ShopItemEntry> items = new List<ShopItemEntry>();

        [Header("Settings")]
        [SerializeField] private bool restockDaily = true;

        // Get all items available in this season
        public List<ShopItemEntry> GetItemsForSeason(Season season, bool seedOnly = true)
        {
            List<ShopItemEntry> result = new List<ShopItemEntry>();

            foreach (var entry in items)
            {
                if (entry.itemData == null) continue;

                bool isAvailable = IsItemAvailableInSeason(entry, season);

                if (isAvailable)
                {
                    if (seedOnly && entry.itemData is SeedItemData seed)
                    {
                        if (seed.cropData != null && seed.cropData.IsValidSeason(season))
                        {
                            result.Add(entry);
                        }
                    }
                    else if (!(entry.itemData is SeedItemData))
                    {
                        result.Add(entry);
                    }
                    else if (!seedOnly)
                    {
                        result.Add(entry);
                    }
                }
            }

            return result;
        }

        private bool IsItemAvailableInSeason(ShopItemEntry entry, Season season)
        {
            if (entry.availableSeasons == null || entry.availableSeasons.Length == 0)
                return true;

            foreach (var s in entry.availableSeasons)
            {
                if (s == season)
                    return true;
            }

            return false;
        }

        // Get all items regardless of season
        public List<ShopItemEntry> GetAllItems()
        {
            return new List<ShopItemEntry>(items);
        }

        // Add item to shop
        public void AddItem(ShopItemEntry entry)
        {
            if (!items.Contains(entry))
            {
                items.Add(entry);
            }
        }
    }
}

