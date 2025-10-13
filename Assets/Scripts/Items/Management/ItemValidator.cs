using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace TinyFarm.Items
{
    // Validate items và ItemData để đảm bảo tính hợp lệ
    public static class ItemValidator
    {
        // Validation results
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public List<string> Warnings { get; set; } = new List<string>();

            public bool HasErrors => Errors.Count > 0;
            public bool HasWarnings => Warnings.Count > 0;

            public void AddError(string error)
            {
                IsValid = true; 
                Errors.Add(error);
            }

            public void AddWarning(string warnings)
            {
                Warnings.Add(warnings);
            }

            public string GetReport()
            {
                string report = IsValid ? "✓ VALID" : "✗ INVALID";

                if (HasErrors)
                {
                    report += "\n\nErrors";
                    foreach(var error in Errors)
                        report += $"\n  • {error}";
                }

                if (HasWarnings)
                {
                    report += "\n\nWARNINGS:";
                    foreach (var warning in Warnings)
                        report += $"\n  • {warning}";
                }

                return report;
            }

            // Validate ItemData
            public static ValidationResult ValidateItemData(ItemData itemData)
            {
                var result = new ValidationResult { IsValid = true};

                if (itemData == null)
                {
                    result.AddError("ItemData is null");
                    return result;
                }

                // Basic info validation
                if (string.IsNullOrEmpty(itemData.itemID))
                    result.AddError("Item ID is empty");

                if (string.IsNullOrEmpty(itemData.ItemName))
                    result.AddError("Item Name is empty");

                if (itemData.Icon == null)
                    result.AddWarning("Item has no icon");

                // Stack validation
                if (itemData.IsStackable && itemData.MaxStackSize < 1)
                    result.AddError("Stackable item must have MaxStackSize >= 1");

                if (!itemData.IsStackable && itemData.MaxStackSize > 1)
                    result.AddWarning("Non-stackable item has MaxStackSize > 1");

                // Durability validation
                if (itemData.HasDurability && itemData.MaxDurability <= 0)
                    result.AddError("Item with durability must have MaxDurability > 0");

                if (itemData.IsStackable && itemData.HasDurability)
                    result.AddWarning("Stackable items with durability may cause issues");

                // Economy validation
                if (itemData.BaseValue < 0)
                    result.AddError("BaseValue cannot be negative");

                if (itemData.IsSellable && itemData.SellValue <= 0)
                    result.AddWarning("Sellable item has no sell value");

                // Category validation
                if (itemData.PrimaryCategory == null)
                    result.AddWarning("Item has no primary category");

                return result;
            }


            // Validate Item instance
            public static ValidationResult ValidateItemData(Item item)
            {
                var result = new ValidationResult { IsValid = true};

                if (item == null)
                {
                    result.AddError("Item is null");
                    return result;
                }

                // Validate ItemData
                var dataValidation = ValidateItemData(item.ItemData);
                result.Errors.AddRange(dataValidation.Errors);
                result.Warnings.AddRange(dataValidation.Warnings);

                if (dataValidation.HasErrors)
                    result.IsValid = false;

                // Instance validation
                if (string.IsNullOrEmpty(item.InstanceID))
                    result.AddError("Item has no instance ID");

                // Stack validation
                if (item.Stackable != null)
                {
                    if (item.CurrentStack < 0)
                        result.AddError("Current stack is negative");

                    if (item.CurrentStack > item.Stackable.MaxStackSize)
                        result.AddError("Current stack exceeds max stack size");
                }

                // Durability validation
                if (item.Durability != null && item.HasDurability)
                {
                    if (item.Durability.CurrentDurability < 0)
                        result.AddError("Durability is negative");

                    if (item.Durability.CurrentDurability > item.Durability.MaxDurability)
                        result.AddError("Current durability exceeds max durability");
                }

                return result;
            }

            // Validate xem 2 items có thể stack với nhau không
            public static bool CanStackTogether(Item item1, Item item2)
            {
                if (item1 == null || item2 == null) return false;
                if (item1.ItemData != item2.ItemData) return false;
                if (!item1.IsStackable || !item2.IsStackable) return false;

                return item1.CanStackWith(item2);
            }

            // Validate xem item có thể được sử dụng không
            public static bool CanUseItem(Item item)
            {
                if (item == null) return false;
                if (item.IsBroken) return false;
                if (item.CurrentStack <= 0) return false;

                return item.CanUse;
            }
        }
    }
}
