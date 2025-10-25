using System;
using UnityEngine;
using TinyFarm.Animation;
using TinyFarm.Items;

namespace TinyFarm.Tools
{
    public class ToolEquipmentController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerAnimationController animController;
        [SerializeField] private AnimationEventHandler eventHandler;

        [Header("Settings")]
        [SerializeField] private bool debugMode = false;

        [Header("Runtime Info (Read Only)")]
        [SerializeField] private ToolItemData currentToolData;
        [SerializeField] private ToolType currentToolType = ToolType.None;
        [SerializeField] private bool hasToolEquipped = false;

        // Events
        public event Action<ToolItemData> OnToolEquipped;
        public event Action<ToolItemData> OnToolUnequipped;
        public event Action<ToolItemData, ToolItemData> OnToolChanged; // (old, new)

        // Properties
        public ToolItemData CurrentTool => currentToolData;
        public ToolType CurrentToolType => currentToolType;
        public bool HasToolEquipped => hasToolEquipped;
        public bool CanEquipTool => !animController.IsActionLocked;
        public int CurrentToolEfficiency => currentToolData?.efficiency ?? 1;

        // ==========================================
        // INITIALIZATION
        // ==========================================

        private void Awake()
        {
            InitializeComponents();
        }

        private void Start()
        {
            ValidateSetup();
            SubscribeToEvents();
            LogDebug("ToolEquipmentController initialized");
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void OnValidate()
        {
            if (animController == null)
                animController = GetComponent<PlayerAnimationController>();

            if (eventHandler == null)
                eventHandler = GetComponent<AnimationEventHandler>();
        }

        private void InitializeComponents()
        {
            if (animController == null)
                animController = GetComponent<PlayerAnimationController>();

            if (eventHandler == null)
                eventHandler = GetComponent<AnimationEventHandler>();

            if (animController == null)
            {
                Debug.LogError("[ToolEquipment] PlayerAnimationController not found!");
                enabled = false;
            }
        }

        private void ValidateSetup()
        {
            if (animController == null)
            {
                Debug.LogError("[ToolEquipment] Missing PlayerAnimationController!");
                enabled = false;
            }

            if (eventHandler == null)
            {
                Debug.LogWarning("[ToolEquipment] AnimationEventHandler not found - sound/events may not work");
            }
        }

        private void SubscribeToEvents()
        {
            if (eventHandler != null)
            {
                eventHandler.OnToolImpactEvent += HandleToolImpact;
                eventHandler.OnSoundEvent += HandleToolSound;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (eventHandler != null)
            {
                eventHandler.OnToolImpactEvent -= HandleToolImpact;
                eventHandler.OnSoundEvent -= HandleToolSound;
            }
        }

        // ==========================================
        // PUBLIC API - EQUIP/UNEQUIP
        // ==========================================

        /// Equip tool từ ToolItemData (called by Hotbar/Inventory)
        /// <param name="toolData">Tool item data từ ScriptableObject</param>
        /// <returns>True nếu equip thành công</returns>
        public bool EquipTool(ToolItemData toolData)
        {
            // Validate
            if (toolData == null)
            {
                Debug.LogWarning("[ToolEquipment] Cannot equip null tool data");
                return false;
            }

            if (toolData.GetItemType() != ItemType.Tool)
            {
                Debug.LogWarning($"[ToolEquipment] Item is not a tool: {toolData.itemName}");
                return false;
            }

            if (!CanEquipTool)
            {
                LogDebug("Cannot equip tool - action locked");
                return false;
            }

            // Check if already equipped
            if (currentToolData == toolData)
            {
                LogDebug($"Tool already equipped: {toolData.itemName}");
                return true;
            }

            // Store old tool for event
            ToolItemData oldTool = currentToolData;

            // Equip new tool
            currentToolData = toolData;
            currentToolType = toolData.toolType;
            hasToolEquipped = true;

            // Update animation controller
            animController.SetCurrentTool(currentToolType);

            // Fire events
            OnToolEquipped?.Invoke(toolData);

            if (oldTool != null)
            {
                OnToolChanged?.Invoke(oldTool, toolData);
            }

            LogDebug($"Equipped tool: {toolData.itemName} ({currentToolType}) [Efficiency: {toolData.efficiency}]");

            return true;
        }

        // Unequip tool hiện tại
        public void UnequipTool()
        {
            if (!hasToolEquipped)
            {
                LogDebug("No tool to unequip");
                return;
            }

            ToolItemData oldTool = currentToolData;

            // Clear tool
            currentToolData = null;
            currentToolType = ToolType.None;
            hasToolEquipped = false;

            // Update animation controller
            animController.SetCurrentTool(ToolType.None);

            // Fire event
            OnToolUnequipped?.Invoke(oldTool);

            LogDebug($"Unequipped tool: {oldTool?.itemName}");
        }

        /// Quick equip bằng ToolType (nếu không có ToolItemData instance)
        /// Useful for testing
        /// <param name="toolType">ToolType enum</param>
        public void EquipToolByType(ToolType toolType)
        {
            if (!CanEquipTool)
            {
                LogDebug("Cannot equip tool - action locked");
                return;
            }

            currentToolType = toolType;
            hasToolEquipped = toolType != ToolType.None;
            currentToolData = null; // No data, just type

            // Update animation controller
            animController.SetCurrentTool(toolType);

            LogDebug($"Equipped tool by type: {toolType}");
        }

        // ==========================================
        // PUBLIC API - USAGE
        // ==========================================

        /// Sử dụng tool hiện tại (trigger animation)
        /// <returns>True nếu sử dụng thành công</returns>
        public bool UseTool()
        {
            if (!hasToolEquipped)
            {
                LogDebug("No tool equipped to use");
                return false;
            }

            if (currentToolData != null && !currentToolData.isUsable)
            {
                LogDebug($"Tool {currentToolData.itemName} is not usable");
                return false;
            }

            if (animController.IsActionLocked)
            {
                LogDebug("Cannot use tool - action locked");
                return false;
            }

            // Trigger animation based on tool type
            bool success = TriggerToolAnimation();

            if (success)
            {
                LogDebug($"Used tool: {currentToolType} [Efficiency: {CurrentToolEfficiency}]");
            }

            return success;
        }

        // Trigger animation dựa vào tool type
        private bool TriggerToolAnimation()
        {
            switch (currentToolType)
            {
                case ToolType.Hoe:
                    return animController.PlayHoeing();

                case ToolType.Watering:
                    return animController.PlayWatering();

                case ToolType.Sickle:
                    return animController.PlaySickle();

                case ToolType.PickUpIdle:
                case ToolType.PickUpRun:
                    return animController.PlayPickUp();

                case ToolType.Seeds:
                    // TODO: Implement planting animation
                    LogDebug("Planting not implemented yet");
                    return false;

                default:
                    Debug.LogWarning($"[ToolEquipment] No animation for tool type: {currentToolType}");
                    return false;
            }
        }

        // ==========================================
        // EVENT HANDLERS
        // ==========================================

        private void HandleToolImpact(AnimationEventData eventData)
        {
            if (!hasToolEquipped)
                return;

            // Tool impact với efficiency
            LogDebug($"Tool impact: {currentToolType} [Efficiency: {CurrentToolEfficiency}]");

            // TODO: Apply tool effect based on efficiency
            // Example: FarmingSystem.ApplyToolEffect(eventData.impactPoint, CurrentToolEfficiency);
        }

        private void HandleToolSound(AnimationEventData eventData)
        {
            if (!hasToolEquipped)
                return;

            // Play tool sound from ToolItemData
            if (currentToolData != null && currentToolData.useSound != null)
            {
                // TODO: Integrate với Sound Manager
                // SoundManager.Instance.PlaySound(currentToolData.useSound);
                LogDebug($"Playing tool sound: {currentToolData.useSound.name}");
            }
        }

        // ==========================================
        // PUBLIC API - GETTERS
        // ==========================================

        // Get ToolItemData hiện tại
        public ToolItemData GetCurrentTool()
        {
            return currentToolData;
        }

        // Get ToolType hiện tại
        public ToolType GetCurrentToolType()
        {
            return currentToolType;
        }

        // Get tool ID hiện tại
        public string GetCurrentToolID()
        {
            return currentToolData?.toolID;
        }

        // Get efficiency của tool hiện tại
        public int GetCurrentToolEfficiency()
        {
            return CurrentToolEfficiency;
        }

        // Check xem có tool equipped không
        public bool IsToolEquipped()
        {
            return hasToolEquipped;
        }

        // Check xem tool cụ thể có đang equipped không
        public bool IsToolEquipped(ToolType toolType)
        {
            return hasToolEquipped && currentToolType == toolType;
        }

        // Check xem có thể dùng tool không
        public bool CanUseTool()
        {
            return hasToolEquipped
                && !animController.IsActionLocked
                && (currentToolData == null || currentToolData.isUsable);
        }

        // ==========================================
        // DEBUG
        // ==========================================

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[ToolEquipment] {message}");
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Debug - Log Current Tool")]
        private void DebugLogCurrentTool()
        {
            Debug.Log("=== TOOL EQUIPMENT STATE ===");
            Debug.Log($"Has Tool Equipped: {hasToolEquipped}");
            Debug.Log($"Current Tool Type: {currentToolType}");

            if (currentToolData != null)
            {
                Debug.Log($"Tool Name: {currentToolData.itemName}");
                Debug.Log($"Tool ID: {currentToolData.toolID}");
                Debug.Log($"Efficiency: {currentToolData.efficiency}");
                Debug.Log($"Use Sound: {(currentToolData.useSound != null ? currentToolData.useSound.name : "None")}");
                Debug.Log($"Use Animation: {(currentToolData.useAnimation != null ? currentToolData.useAnimation.name : "None")}");
            }
            else
            {
                Debug.Log("Tool Data: None");
            }

            Debug.Log($"Can Equip: {CanEquipTool}");
            Debug.Log($"Can Use: {CanUseTool()}");
        }

        [ContextMenu("Test - Equip Hoe (by type)")]
        private void TestEquipHoe()
        {
            EquipToolByType(ToolType.Hoe);
        }

        [ContextMenu("Test - Equip Watering (by type)")]
        private void TestEquipWatering()
        {
            EquipToolByType(ToolType.Watering);
        }

        [ContextMenu("Test - Unequip Tool")]
        private void TestUnequipTool()
        {
            UnequipTool();
        }

        [ContextMenu("Test - Use Tool")]
        private void TestUseTool()
        {
            UseTool();
        }
#endif
    }
}

