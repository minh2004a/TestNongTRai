using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.PlayerInput
{
    public enum InputState
    {
        Gameplay,       // Normal gameplay - tất cả input enabled
        UI,             // UI mode (inventory open) - chỉ UI input
        Cutscene,       // Cutscene/Dialogue - no input
        Disabled        // Completely disabled
    }
}

