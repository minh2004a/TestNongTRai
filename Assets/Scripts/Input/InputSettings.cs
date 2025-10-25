using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TinyFarm.PlayerInput
{
    // ScriptableObject config cho tất cả input settings
    // Tạo: Create → TinyFarm → Input Settings
    [CreateAssetMenu(fileName = "InputSettings", menuName = "TinyFarm/Input Settings", order = 0)]
    public class InputSettings : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Horizontal axis name (default: Horizontal)")]
        public string horizontalAxis = "Horizontal";

        [Tooltip("Vertical axis name (default: Vertical)")]
        public string verticalAxis = "Vertical";

        [Tooltip("Dead zone cho joystick/gamepad")]
        [Range(0f, 0.5f)]
        public float deadZone = 0.1f;

        [Header("Actions")]
        [Tooltip("Interact key (NPC, chest, pick up)")]
        public KeyCode interactKey = KeyCode.E;

        [Tooltip("Sleep/Wake up key")]
        public KeyCode sleepKey = KeyCode.R;

        [Tooltip("Inventory key")]
        public KeyCode inventoryKey = KeyCode.Tab;

        [Header("Tool Usage")]
        [Tooltip("Use tool key (chuột trái)")]
        public KeyCode useToolKey = KeyCode.Mouse0;

        [Header("Hotbar")]
        [Tooltip("Hotbar keys (0-9 for slots)")]

        public KeyCode[] hotbarKeys = new KeyCode[]
        {
            KeyCode.Alpha1,
            KeyCode.Alpha2,
            KeyCode.Alpha3,
            KeyCode.Alpha4,
            KeyCode.Alpha5,
            KeyCode.Alpha6,
            KeyCode.Alpha7,
            KeyCode.Alpha8,
            KeyCode.Alpha9,
            KeyCode.Alpha0
        };

        [Header("Input Buffering")]
        [Tooltip("Enable input buffering")]
        public bool enableBuffering = true;

        [Tooltip("Buffer duration (seconds)")]
        [Range(0.1f, 1f)]
        public float bufferDuration = 0.2f;

        [Header("Input Blocking")]
        [Tooltip("Block movement khi action locked")]
        public bool blockMovementWhenLocked = true;

        [Tooltip("Block tool use khi action locked")]
        public bool blockToolUseWhenLocked = true;

        // ==========================================
        // VALIDATION
        // ==========================================

        private void OnValidate()
        {
            // Ensure hotbar has at least 1 key
            if (hotbarKeys == null || hotbarKeys.Length == 0)
            {
                hotbarKeys = new KeyCode[] { KeyCode.Alpha1 };
            }

            // Clamp values
            deadZone = Mathf.Clamp(deadZone, 0f, 0.5f);
            bufferDuration = Mathf.Clamp(bufferDuration, 0.1f, 1f);
        }

        // ==========================================
        // UTILITY
        // ==========================================

        // Get hotbar slot index từ KeyCode
        public int GetHotbarSlotIndex(KeyCode key)
        {
            for (int i = 0; i < hotbarKeys.Length; i++)
            {
                if (hotbarKeys[i] == key)
                    return i;
            }
            return -1;
        }
    }
}

