using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.Items.UI
{
    public interface ISlotUIBase
    {
        /// The inventory slot data
        InventorySlot Slot { get; }

        /// The slot index in inventory
        int SlotIndex { get; }

        /// Is the slot empty?
        bool IsEmpty { get; }

        /// Is the slot currently hovered?
        bool IsHovered { get; }

        /// Is the slot currently selected?
        bool IsSelected { get; }

        /// Update the UI display
        void UpdateUI();

        /// Select this slot
        void Select();

        /// Deselect this slot
        void Deselect();
    }

}
