using System;
using System.Collections;
using System.Collections.Generic;
using TinyFarm.Animation;
using TinyFarm.Items;
using TinyFarm.Tools;
using UnityEngine;

namespace TinyFarm.PlayerInput 
{
    // Main input handler cho player
    // Handle tất cả input và forward đến các systems tương ứng
    // Architecture: Hybrid (Direct calls cho movement, Events cho actions)
    // Complexity: HIGH (~32 methods)
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private ToolEquipmentController toolEquipment;
        [SerializeField] private PlayerAnimationController animController;

        [Header("Settings")]
        [SerializeField] private InputSettings inputSettings;

        [Header("Runtime State")]
        [SerializeField] private InputState currentInputState = InputState.Gameplay;
        [SerializeField] private bool isInputEnabled = true;

        [Header("Debug")]
        [SerializeField] private bool debugMode = false;
        [SerializeField]
        private bool showInputDebug = false;

        // Input buffering
        private Queue<BufferedInput> inputBuffer = new Queue<BufferedInput>();
        private const int MAX_BUFFER_SIZE = 5;

        // Cached input values
        private Vector2 moveInput;
        private Vector2 lastMoveInput;

        // Events - Discrete actions
        public event Action<Vector2> OnInteractPressed;     // Para: interaction point
        public event Action OnSleepPressed;
        public event Action OnInventoryToggled;
        public event Action<int> OnHotbarSlotSelected;      // Para: slot index

        // Properties
        public InputState CurrentInputState => currentInputState;
        public bool IsInputEnabled => isInputEnabled;
        public Vector2 MoveInput => moveInput;
        public bool IsMoving => moveInput.sqrMagnitude > inputSettings.deadZone;

        // ==========================================
        // INITIALIZATION
        // ==========================================

        private void Awake()
        {
            InitializeComponents();
            ValidateSettings();
        }

        private void Start()
        {
            SetInputState(InputState.Gameplay);
            LogDebug("PlayerInputHandler initialized");
        }

        private void OnValidate()
        {
            if (playerMovement == null)
                playerMovement = GetComponent<PlayerMovement>();

            if (toolEquipment == null)
                toolEquipment = GetComponent<ToolEquipmentController>();

            if (animController == null)
                animController = GetComponent<PlayerAnimationController>();
        }

        private void InitializeComponents()
        {
            if (playerMovement == null)
                playerMovement = GetComponent<PlayerMovement>();

            if (toolEquipment == null)
                toolEquipment = GetComponent<ToolEquipmentController>();

            if (animController == null)
                animController = GetComponent<PlayerAnimationController>();
        }

        private void ValidateSettings()
        {
            if (inputSettings == null)
            {
                Debug.LogError("[PlayerInput] InputSettings not assigned! Input will not work.");
                enabled = false;
                return;
            }

            if (playerMovement == null)
            {
                Debug.LogWarning("[PlayerInput] PlayerMovement not found!");
            }

            if (toolEquipment == null)
            {
                Debug.LogWarning("[PlayerInput] ToolEquipmentController not found!");
            }

            if (animController == null)
            {
                Debug.LogWarning("[PlayerInput] PlayerAnimationController not found!");
            }
        }

        // ==========================================
        // UPDATE LOOP
        // ==========================================

        private void Update()
        {
            if (!isInputEnabled)
                return;

            // Process buffered inputs first
            ProcessBufferedInputs();

            // Read inputs based on state
            switch (currentInputState)
            {
                case InputState.Gameplay:
                    ReadMovementInput();
                    ReadToolInput();
                    ReadActionInput();
                    ReadHotbarInput();
                    break;

                case InputState.UI:
                    ReadUIInput();
                    break;

                case InputState.Cutscene:
                case InputState.Disabled:
                    // No input
                    break;
            }
        }

        // ==========================================
        // MOVEMENT INPUT (Direct Call)
        // ==========================================

        private void ReadMovementInput()
        {
            // Check if movement blocked
            if (inputSettings.blockMovementWhenLocked && IsActionLocked())
            {
                moveInput = Vector2.zero;

                if (playerMovement != null)
                {
                    playerMovement.SetMoveInput(Vector2.zero);
                }
                return;
            }

            // Read horizontal & vertical axes
            float h = UnityEngine.Input.GetAxisRaw(inputSettings.horizontalAxis);
            float v = UnityEngine.Input.GetAxisRaw(inputSettings.verticalAxis);

            moveInput = new Vector2(h, v);

            // Apply dead zone
            if (moveInput.sqrMagnitude < inputSettings.deadZone * inputSettings.deadZone)
            {
                moveInput = Vector2.zero;
            }

            // Normalize diagonal movement
            if (moveInput.sqrMagnitude > 1f)
            {
                moveInput.Normalize();
            }

            // Send to PlayerMovement
            if (playerMovement != null)
            {
                playerMovement.SetMoveInput(moveInput);
            }

            // Track for debug
            if (moveInput.sqrMagnitude > 0.01f)
            {
                lastMoveInput = moveInput;
            }

            LogInputDebug($"Move: ({moveInput.x:F2}, {moveInput.y:F2})");
        }

        // ==========================================
        // TOOL INPUT (Direct Call + Buffering)
        // ==========================================

        private void ReadToolInput()
        {
            // Use Tool (Mouse0)
            if (UnityEngine.Input.GetKeyDown(inputSettings.useToolKey))
            {
                HandleToolUse();
            }
        }

        private void HandleToolUse()
        {
            if (toolEquipment == null)
                return;

            // Check if can use tool
            if (CanUseTool())
            {
                bool success = toolEquipment.UseTool();

                if (success)
                {
                    LogDebug("Used tool");
                }
                else
                {
                    LogDebug("Failed to use tool");
                }
            }
            else
            {
                // Buffer input if enabled
                if (inputSettings.enableBuffering)
                {
                    BufferInput(InputAction.UseTool);
                    LogDebug("Buffered tool use");
                }
            }
        }

        private bool CanUseTool()
        {
            if (inputSettings.blockToolUseWhenLocked && IsActionLocked())
                return false;

            if (toolEquipment == null)
                return false;

            return toolEquipment.CanUseTool();
        }

        // ==========================================
        // ACTION INPUT (Events)
        // ==========================================

        private void ReadActionInput()
        {
            // Interact (E)
            if (UnityEngine.Input.GetKeyDown(inputSettings.interactKey))
            {
                HandleInteract();
            }

            // Sleep/Wake (R)
            if (UnityEngine.Input.GetKeyDown(inputSettings.sleepKey))
            {
                HandleSleep();
            }

            // Inventory (Tab)
            if (UnityEngine.Input.GetKeyDown(inputSettings.inventoryKey))
            {
                HandleInventoryToggle();
            }
        }

        private void HandleInteract()
        {
            if (IsActionLocked())
            {
                if (inputSettings.enableBuffering)
                {
                    BufferInput(InputAction.Interact);
                    LogDebug("Buffered interact");
                }
                return;
            }

            // Fire event với position (for raycasting/overlap check)
            Vector2 interactPoint = (Vector2)transform.position + GetFacingDirection();
            OnInteractPressed?.Invoke(interactPoint);

            LogDebug($"Interact pressed at {interactPoint}");
        }

        private void HandleSleep()
        {
            if (animController == null)
                return;

            // Toggle sleep
            if (animController.CurrentState == AnimationState.Sleep)
            {
                animController.WakeUp();
                LogDebug("Wake up");
            }
            else
            {
                animController.PlaySleep();
                LogDebug("Sleep");
            }

            OnSleepPressed?.Invoke();
        }

        private void HandleInventoryToggle()
        {
            // Toggle between Gameplay and UI state
            if (currentInputState == InputState.Gameplay)
            {
                SetInputState(InputState.UI);
            }
            else if (currentInputState == InputState.UI)
            {
                SetInputState(InputState.Gameplay);
            }

            OnInventoryToggled?.Invoke();
            LogDebug($"Inventory toggled - State: {currentInputState}");
        }

        // ==========================================
        // HOTBAR INPUT (Events)
        // ==========================================

        private void ReadHotbarInput()
        {
            // Check hotbar keys (1-0)
            for (int i = 0; i < inputSettings.hotbarKeys.Length; i++)
            {
                if (UnityEngine.Input.GetKeyDown(inputSettings.hotbarKeys[i]))
                {
                    HandleHotbarSelection(i);
                    break;
                }
            }
        }

        private void HandleHotbarSelection(int slotIndex)
        {
            if (IsActionLocked())
            {
                if (inputSettings.enableBuffering)
                {
                    BufferInput(InputAction.EquipTool, slotIndex);
                    LogDebug($"Buffered hotbar slot {slotIndex}");
                }
                return;
            }

            // Fire event to Hotbar UI
            OnHotbarSlotSelected?.Invoke(slotIndex);

            LogDebug($"Hotbar slot {slotIndex} selected");
        }

        // ==========================================
        // UI INPUT
        // ==========================================

        private void ReadUIInput()
        {
            // In UI mode, only allow inventory toggle
            if (UnityEngine.Input.GetKeyDown(inputSettings.inventoryKey))
            {
                HandleInventoryToggle();
            }

            // Clear movement input
            moveInput = Vector2.zero;
            if (playerMovement != null)
            {
                playerMovement.SetMoveInput(Vector2.zero);
            }
        }

        // ==========================================
        // INPUT BUFFERING SYSTEM
        // ==========================================

        private void BufferInput(InputAction action, object data = null)
        {
            if (!inputSettings.enableBuffering)
                return;

            // Check buffer size limit
            if (inputBuffer.Count >= MAX_BUFFER_SIZE)
            {
                inputBuffer.Dequeue(); // Remove oldest
            }

            // Add new buffered input
            var buffered = new BufferedInput(action, Time.time, data);
            inputBuffer.Enqueue(buffered);

            LogDebug($"Buffered input: {action}");
        }

        private void ProcessBufferedInputs()
        {
            if (!inputSettings.enableBuffering || inputBuffer.Count == 0)
                return;

            // Process buffered inputs if no longer locked
            if (!IsActionLocked())
            {
                while (inputBuffer.Count > 0)
                {
                    var buffered = inputBuffer.Dequeue();

                    // Check if expired
                    if (buffered.IsExpired(Time.time, inputSettings.bufferDuration))
                    {
                        LogDebug($"Buffered input expired: {buffered.action}");
                        continue;
                    }

                    // Execute buffered action
                    ExecuteBufferedAction(buffered);

                    // Only process one buffered input per frame
                    break;
                }
            }
            else
            {
                // Clear expired inputs
                ClearExpiredBufferedInputs();
            }
        }

        private void ExecuteBufferedAction(BufferedInput buffered)
        {
            LogDebug($"Executing buffered input: {buffered.action}");

            switch (buffered.action)
            {
                case InputAction.UseTool:
                    if (toolEquipment != null && toolEquipment.CanUseTool())
                    {
                        toolEquipment.UseTool();
                    }
                    break;

                case InputAction.Interact:
                    Vector2 interactPoint = (Vector2)transform.position + GetFacingDirection();
                    OnInteractPressed?.Invoke(interactPoint);
                    break;

                case InputAction.EquipTool:
                    if (buffered.data is int slotIndex)
                    {
                        OnHotbarSlotSelected?.Invoke(slotIndex);
                    }
                    break;
            }
        }

        private void ClearExpiredBufferedInputs()
        {
            int originalCount = inputBuffer.Count;

            // Create temp list to avoid modifying queue while iterating
            var temp = new List<BufferedInput>();

            while (inputBuffer.Count > 0)
            {
                var buffered = inputBuffer.Dequeue();

                if (!buffered.IsExpired(Time.time, inputSettings.bufferDuration))
                {
                    temp.Add(buffered);
                }
            }

            // Re-enqueue non-expired inputs
            foreach (var buffered in temp)
            {
                inputBuffer.Enqueue(buffered);
            }

            if (temp.Count < originalCount)
            {
                LogDebug($"Cleared {originalCount - temp.Count} expired buffered inputs");
            }
        }

        // Clear all buffered inputs
        public void ClearInputBuffer()
        {
            inputBuffer.Clear();
            LogDebug("Input buffer cleared");
        }

        // ==========================================
        // INPUT STATE MANAGEMENT
        // ==========================================

        // Set input state (Gameplay, UI, Cutscene, Disabled)
        public void SetInputState(InputState newState)
        {
            if (currentInputState == newState)
                return;

            InputState oldState = currentInputState;
            currentInputState = newState;

            OnInputStateChanged(oldState, newState);

            LogDebug($"Input state: {oldState} → {newState}");
        }

        private void OnInputStateChanged(InputState oldState, InputState newState)
        {
            // Handle state transitions
            switch (newState)
            {
                case InputState.Gameplay:
                    // Enable gameplay input
                    break;

                case InputState.UI:
                    // Stop movement when entering UI
                    moveInput = Vector2.zero;
                    if (playerMovement != null)
                    {
                        playerMovement.SetMoveInput(Vector2.zero);
                    }
                    break;

                case InputState.Cutscene:
                case InputState.Disabled:
                    // Clear all input
                    moveInput = Vector2.zero;
                    ClearInputBuffer();
                    if (playerMovement != null)
                    {
                        playerMovement.SetMoveInput(Vector2.zero);
                    }
                    break;
            }
        }

        // Enable/disable input completely
        public void SetInputEnabled(bool enabled)
        {
            isInputEnabled = enabled;

            if (!enabled)
            {
                moveInput = Vector2.zero;
                ClearInputBuffer();

                if (playerMovement != null)
                {
                    playerMovement.SetMoveInput(Vector2.zero);
                }
            }

            LogDebug($"Input enabled: {enabled}");
        }

        // ==========================================
        // UTILITY METHODS
        // ==========================================

        private bool IsActionLocked()
        {
            if (animController == null)
                return false;

            return animController.IsActionLocked;
        }

        private Vector2 GetFacingDirection()
        {
            if (animController != null)
            {
                return animController.CurrentDirection switch
                {
                    Direction.Up => Vector2.up,
                    Direction.Down => Vector2.down,
                    Direction.Side => animController.IsFacingLeft ? Vector2.left : Vector2.right,
                    _ => Vector2.down
                };
            }

            // Fallback to last move input
            return lastMoveInput.sqrMagnitude > 0.01f ? lastMoveInput.normalized : Vector2.down;
        }

        // ==========================================
        // PUBLIC API
        // ==========================================

        // Get current move input vector
        public Vector2 GetMoveInput()
        {
            return moveInput;
        }

        // Check if player is currently moving
        public bool IsPlayerMoving()
        {
            return IsMoving;
        }

        // Manually trigger tool use (for AI/testing)
        public void TriggerToolUse()
        {
            HandleToolUse();
        }

        /// <summary>
        /// Manually trigger interact (for AI/testing)
        /// </summary>
        public void TriggerInteract()
        {
            HandleInteract();
        }

        // ==========================================
        // DEBUG
        // ==========================================

        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[PlayerInput] {message}");
            }
        }

        private void LogInputDebug(string message)
        {
            if (showInputDebug)
            {
                Debug.Log($"[Input] {message}");
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (!showInputDebug)
                return;

            GUIStyle style = new GUIStyle(GUI.skin.box);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 11;
            style.normal.textColor = Color.white;

            string info = $@"=== INPUT DEBUG ===
State: {currentInputState}
Enabled: {isInputEnabled}
Action Locked: {IsActionLocked()}

Movement:
  Input: ({moveInput.x:F2}, {moveInput.y:F2})
  Is Moving: {IsMoving}
  Facing: {GetFacingDirection()}

Buffering:
  Enabled: {inputSettings.enableBuffering}
  Buffer Count: {inputBuffer.Count}/{MAX_BUFFER_SIZE}
  Buffer Duration: {inputSettings.bufferDuration:F1}s

Keys:
  Interact: {inputSettings.interactKey}
  Sleep: {inputSettings.sleepKey}
  Inventory: {inputSettings.inventoryKey}
  Use Tool: {inputSettings.useToolKey}
";

            GUI.Box(new Rect(Screen.width - 310, 10, 300, 300), info, style);
        }

        [ContextMenu("Debug - Log Input State")]
        private void DebugLogInputState()
        {
            Debug.Log("=== INPUT STATE ===");
            Debug.Log($"State: {currentInputState}");
            Debug.Log($"Enabled: {isInputEnabled}");
            Debug.Log($"Move Input: {moveInput}");
            Debug.Log($"Is Moving: {IsMoving}");
            Debug.Log($"Action Locked: {IsActionLocked()}");
            Debug.Log($"Buffer Count: {inputBuffer.Count}");
        }

        [ContextMenu("Test - Toggle Input State")]
        private void TestToggleInputState()
        {
            SetInputState(currentInputState == InputState.Gameplay ? InputState.UI : InputState.Gameplay);
        }

        [ContextMenu("Test - Clear Buffer")]
        private void TestClearBuffer()
        {
            ClearInputBuffer();
        }
#endif
    }
}


