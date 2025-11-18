using System;
using UnityEngine;
using TinyFarm.Animation;
using TinyFarm.Items;
using TinyFarm.PlayerInput;
using TinyFarm.Farming;

namespace TinyFarm.Tools
{
    public class ToolEquipmentController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerAnimationController animController;
        [SerializeField] private AnimationEventHandler eventHandler;
        [SerializeField] private FarmingController farmingController;

        [Header("Settings")]
        [SerializeField] private bool debugMode = true;

        [Header("Runtime Info (Read Only)")]
        [SerializeField] private ToolItemData currentTool;
        [SerializeField] private ToolType currentToolType = ToolType.None;
        [SerializeField] private bool hasToolEquipped = false;

        // Events
        public event Action<ToolItemData> OnToolEquipped;
        public event Action<ToolItemData> OnToolUnequipped;
        public event Action<ToolItemData, ToolItemData> OnToolChanged; // (old, new)

        // Properties
        public ToolItemData CurrentTool => currentTool;
        public ToolType CurrentToolType => currentToolType;
        public bool HasToolEquipped => hasToolEquipped;
        public bool CanEquipTool => !animController.IsActionLocked;
        public int CurrentToolEfficiency => currentTool?.efficiency ?? 1;

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

            if (farmingController == null)
                farmingController = GetComponent<FarmingController>();
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
            var holding = FindObjectOfType<ItemHoldingController>();
            if (holding != null && holding.IsHoldingItem)
                holding.UnequipItem();
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
            if (currentTool == toolData)
            {
                LogDebug($"Tool already equipped: {toolData.itemName}");
                return true;
            }

            // Store old tool for event
            ToolItemData oldTool = currentTool;

            // ✅ EXIT pickup state from old tool if needed
            if (oldTool != null && IsCarryableItem(oldTool))
            {
                OnUnequipCarryableItem();
            }

            // Equip new tool
            currentTool = toolData;
            currentToolType = toolData.toolType;
            hasToolEquipped = true;

            // Update animation controller
            animController.SetCurrentTool(currentToolType);

            // ✅ ENTER pickup state for new tool if needed
            if (IsCarryableItem(toolData))
            {
                OnEquipCarryableItem();
            }

            // Fire events
            OnToolEquipped?.Invoke(toolData);

            if (oldTool != null)
            {
                OnToolChanged?.Invoke(oldTool, toolData);
            }

            LogDebug($"✅ Equipped tool: {toolData.itemName} ({currentToolType}) [Efficiency: {toolData.efficiency}]");

            return true;
        }

        // Unequip tool hiện tại
        public void UnequipTool()
        {
            var holding = FindObjectOfType<ItemHoldingController>();
            if (holding != null && holding.IsHoldingItem)
                holding.UnequipItem();
            if (!hasToolEquipped)
            {
                LogDebug("No tool to unequip");
                return;
            }

            ToolItemData oldTool = currentTool;

            // ✅ EXIT pickup state if this was a carryable item
            if (oldTool != null && IsCarryableItem(oldTool))
            {
                OnUnequipCarryableItem();
            }

            // Clear tool
            currentTool = null;
            currentToolType = ToolType.None;
            hasToolEquipped = false;

            // Update animation controller
            animController.SetCurrentTool(ToolType.None);

            // Fire event
            OnToolUnequipped?.Invoke(oldTool);

            LogDebug($"✅ Unequipped tool: {oldTool?.itemName}");
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
            currentTool = null; // No data, just type

            // Update animation controller
            animController.SetCurrentTool(toolType);

            LogDebug($"Equipped tool by type: {toolType}");
        }

        // Gọi khi equip item/tool có visual (carrot, etc.)
        public void OnEquipCarryableItem()
        {
            if (animController != null)
            {
                animController.PlayPickUp();
                Debug.Log("✅ [ToolEquipment] Entered PickUp state");
            }
        }

        // Gọi khi unequip item/tool có visual
        public void OnUnequipCarryableItem()
        {
            if (animController != null)
            {
                animController.ExitPickUpState();
                Debug.Log("✅ [ToolEquipment] Exited PickUp state");
            }
        }

        // ==========================================
        // PUBLIC API - USAGE
        // ==========================================

        // Check if item should trigger pickup animation
        private bool IsCarryableItem(ToolItemData toolData)
        {
            if (toolData == null)
                return false;

            // ✅ Thêm logic của bạn ở đây
            // Option 1: Check tool type
            if (toolData.toolType == ToolType.PickUpIdle ||
                toolData.toolType == ToolType.PickUpRun)
            {
                return true;
            }

            // Option 2: Check if tool has pickup animation flag (nếu bạn có field này)
            // return toolData.hasPickupAnimation;

            // Option 3: Check tool name (temporary solution)
            // return toolData.itemName.Contains("Carrot") || toolData.itemName.Contains("Seed");

            return false;
        }

        // Sử dụng tool hiện tại (trigger animation)
        // <returns>True nếu sử dụng thành công</returns>
        public bool UseTool()
        {
            Debug.Log($"🟠 [TRACE] ToolEquipmentController.UseTool() CALLED - hasToolEquipped={hasToolEquipped}, CanUseTool={CanUseTool()}, Time.frameCount={Time.frameCount}");
            Debug.Log($"    hasToolEquipped={hasToolEquipped}");
            Debug.Log($"    CanUseTool={CanUseTool()}");
            Debug.Log($"    currentTool={currentTool?.itemName ?? "null"}");
            Debug.Log($"    Time.frameCount={Time.frameCount}");
            if (currentTool == null)
            {
                Debug.Log($"🟠 [TRACE] UseTool FAILED - currentTool is null");

                return false;
            }

            if (!CanUseTool())
            {
                Debug.Log($"🟠 [TRACE] UseTool FAILED - CanUseTool is false");

                return false;
            }

            // Trigger animation
            bool animationStarted = false;

            switch (currentTool.toolType)
            {
                case ToolType.Hoe:
                    Debug.Log($"🟠 [TRACE] Calling animController.PlayHoeing()");

                    animationStarted = animController.PlayHoeing();
                    Debug.Log($"🟠 [TRACE] PlayHoeing() returned: {animationStarted}");
                    break;
                case ToolType.Watering:
                    animationStarted = animController.PlayWatering();
                    break;
                case ToolType.Sickle:
                    animationStarted = animController.PlaySickle();
                    break;
                case ToolType.Shovel:
                    animationStarted = animController.PlayShovel();
                    break;
                default:
                    break;
            }

            if (animationStarted)
            {
                        Debug.Log($"🟠 [TRACE] UseTool SUCCESSFUL - animation started");

                return true;
            }
            else
            {
                        Debug.Log($"🟠 [TRACE] UseTool FAILED - animation didn't start");

                return false;
            }

        }
        
        // ==========================================
        // EVENT HANDLERS
        // ==========================================

        private void HandleToolImpact(AnimationEventData eventData)
        {
            Debug.Log("🔨 [ToolEquipment] HandleToolImpact called!");

            if (!hasToolEquipped)
                return;

            var farm = GetComponent<FarmingController>();

            if (farm == null)
            {
                farm = FindObjectOfType<FarmingController>();
            }

            if (farm != null)
            {
                Debug.Log("🔨 Calling FarmingController.ProcessToolImpact()");
                farm.ProcessToolImpact();
            }

            LogDebug($"Tool impact: {currentToolType} [Efficiency: {CurrentToolEfficiency}]");
        }

        private void HandleToolSound(AnimationEventData eventData)
        {
            if (!hasToolEquipped)
                return;

            // Play tool sound from ToolItemData
            if (currentTool != null && currentTool.useSound != null)
            {
                // TODO: Integrate với Sound Manager
                // SoundManager.Instance.PlaySound(currentToolData.useSound);
                LogDebug($"Playing tool sound: {currentTool.useSound.name}");
            }
        }

        // ==========================================
        // PUBLIC API - GETTERS
        // ==========================================

        // Get ToolItemData hiện tại
        public ToolItemData GetCurrentTool()
        {
            return currentTool;
        }

        // Get ToolType hiện tại
        public ToolType GetCurrentToolType()
        {
            return currentToolType;
        }

        // Get tool ID hiện tại
        public string GetCurrentToolID()
        {
            return currentTool?.toolID;
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
            bool hasEquipped = hasToolEquipped;
            bool notLocked = !animController.IsActionLocked;
            bool isUsable = currentTool == null || currentTool.isUsable;
            
            bool canUse = hasEquipped && notLocked && isUsable;
            
            // DEBUG: Log chi tiết
            if (!canUse)
            {
                Debug.LogWarning($"[CanUseTool] ❌ CANNOT USE TOOL!");
                Debug.LogWarning($"  hasToolEquipped={hasEquipped}");
                Debug.LogWarning($"  isActionLocked={animController.IsActionLocked}");
                Debug.LogWarning($"  currentTool={currentTool?.itemName ?? "null"}");
                Debug.LogWarning($"  isUsable={isUsable}");
            }
    
            return canUse;
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

            if (currentTool != null)
            {
                Debug.Log($"Tool Name: {currentTool.itemName}");
                Debug.Log($"Tool ID: {currentTool.toolID}");
                Debug.Log($"Efficiency: {currentTool.efficiency}");
                Debug.Log($"Use Sound: {(currentTool.useSound != null ? currentTool.useSound.name : "None")}");
                Debug.Log($"Use Animation: {(currentTool.useAnimation != null ? currentTool.useAnimation.name : "None")}");
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

