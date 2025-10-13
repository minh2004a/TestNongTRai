using System.Collections;
using System.Collections.Generic;
using TinyFarm.Items;
using UnityEngine;

public static class ItemCreationExtensions
{
    /// Quick create item
    public static Item Create(this ItemDatabase database, string itemID)
    {
        return ItemManager.Instance?.CreateItem(itemID);
    }

    /// Quick create with quantity
    public static Item Create(this ItemDatabase database, string itemID, int quantity)
    {
        return ItemManager.Instance?.CreateItem(itemID, quantity);
    }
}
