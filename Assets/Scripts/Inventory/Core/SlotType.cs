using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items
{
    public enum SlotType
    {
        Normal,         // Chấp nhận mọi item
        ToolOnly,       // Chỉ tools
        SeedOnly,       // Chỉ seeds
        CropOnly,       // Chỉ crops
        EquipmentOnly,  // Chỉ equipment
        MaterialOnly    // Chỉ crafting materials
    }
}

