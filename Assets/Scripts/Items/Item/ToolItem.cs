using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;

namespace TinyFarm.Items
{
    [Serializable]
    public class ToolItem : Item
    {
        // Tool-specific data
        private ToolItemData toolData;

        // Properties
        public ToolType ToolType => toolData?.toolType ?? ToolType.None;
        
        // Events
        public event Action<ToolItem, Vector3> OnToolUsed;

        // Constructor
        public ToolItem(ToolItemData data) : base(data)
        {
            this.toolData = data;
        }

        public ToolItem(ToolItem other) : base(other)
        {
            this.toolData = other.toolData;
        }

        // Sử dụng tool tại vị trí
        public bool UseTool(Vector3 position)
        {
            if (!CanUse)
            {
                return false;
            }

            // Giảm durability
            if (HasDurability)
            {
                Durability.Damage(toolData.durabilityLossPerUse);
            }

            OnToolUsed?.Invoke(this, position);
            return true;
        }
        public override string ToString()
        {
            return $"{Name} ({Durability.DurabilityPercent:F0}%)";
        }

    }
}

