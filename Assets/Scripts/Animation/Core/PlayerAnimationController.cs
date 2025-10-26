using System;
using System.Collections;
using System.Collections.Generic;
using TinyFarm.Animation;
using TinyFarm.Items;
using UnityEngine;

namespace TinyFarm.Animation
{
    // Core animation controller cho player character
    // Quản lý tất cả animations: movement, tool actions, transitions
    // Tích hợp với Animator Controller và các helper classes
    // Complexity: HIGH (~45 methods)

    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerAnimationController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Configurations")]
        [SerializeField] private AnimationSettings settings;
        [SerializeField] private ToolAnimationConfig[] toolConfigs;

        [Header("Settings")]
        [SerializeField] private bool debugMode = false;
        [SerializeField] private float minMoveThreshold = 0.01f;

        [Header("Runtime Info")]
        [SerializeField, ReadOnly] private AnimationState currentState = AnimationState.Idle;
        [SerializeField, ReadOnly] private AnimationState previousState = AnimationState.Idle;
        [SerializeField, ReadOnly] private Direction currentDirection = Direction.Down;
        [SerializeField, ReadOnly] private bool isActionLocked = false;

        // Private components
        private AnimationParameterCache paramCache;
        private AnimationStateValidator stateValidator;
        private ToolAnimationMapper toolMapper;
        private ToolType currentToolType = ToolType.None;


        // Runtime tracking
        private Vector2 lastDirectionVector = Vector2.down;
        private Coroutine actionCoroutine;
        private float actionStartTime;
        private float actionDuration;

        // Animator parameter names (constants)
        private const string PARAM_STATE = "State";
        private const string PARAM_HORIZONTAL = "Horizontal";
        private const string PARAM_VERTICAL = "Vertical";

        // Events
        public event Action<AnimationState> OnStateChanged;
        public event Action OnActionComplete;
        public event Action<AnimationState> OnToolActionStarted;


        // PROPERTIES
        public AnimationState CurrentState => currentState;
        public AnimationState PreviousState => previousState;
        public Direction CurrentDirection => currentDirection;
        public bool IsActionLocked => isActionLocked;
        public bool IsFacingLeft => spriteRenderer.flipX;
        public bool IsMoving => currentState == AnimationState.Running;
        public Animator Animator => animator;


        private void Awake()
        {
            InitializeComponents();
            InitializeHelperClasses();
        }

        private void Start()
        {
            ValidateSetup();
            SetInitialState();
        }

        private void InitializeComponents()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void InitializeHelperClasses()
        {
            // Initialize AnimationParameterCache
            paramCache = new AnimationParameterCache();
            paramCache.Initialize(animator);

            // Initialize AnimationStateValidator
            stateValidator = new AnimationStateValidator();
            stateValidator.Initialize();

            // Initialize ToolAnimationMapper
            toolMapper = new ToolAnimationMapper();
            if (toolConfigs != null && toolConfigs.Length > 0)
            {
                toolMapper.Initialize(toolConfigs);
                LogDebug($"Loaded {toolConfigs.Length} tool configurations");
            }
            else
            {
                Debug.LogWarning("[PlayerAnimController] No ToolAnimationConfig assigned!");
            }
        }

        private void ValidateSetup()
        {
            if (animator == null)
            {
                Debug.LogError("[PlayerAnimController] Animator is missing!");
                enabled = false;
                return;
            }

            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError("[PlayerAnimController] Animator Controller not assigned!");
                enabled = false;
                return;
            }

            if (settings == null)
            {
                Debug.LogWarning("[PlayerAnimController] AnimationSettings not assigned! Using defaults.");
            }
        }

        private void SetInitialState()
        {
            currentState = AnimationState.Idle;
            currentDirection = Direction.Down;
            lastDirectionVector = Vector2.down;

            // Set animator to idle state
            SetAnimatorState(AnimationState.Idle);
            UpdateDirectionParameters(lastDirectionVector);

            LogDebug("PlayerAnimationController initialized");
        }

        // MOVEMENT ANIMATIONS

        // Play Idle animation với hướng hiện tại
        public void PlayIdle()
        {
            if (isActionLocked)
            {
                LogDebug("Cannot play idle - action locked");
                return;
            }

            TransitionToState(AnimationState.Idle);
            UpdateDirectionParameters(lastDirectionVector);
        }

        // Play movement animation (running)
        /// <param name="direction">Direction vector từ input</param>
        public void PlayMovement(Vector2 direction)
        {
            if (isActionLocked)
            {
                LogDebug("Cannot play movement - action locked");
                return;
            }

            // Check if actually moving
            if (direction.sqrMagnitude < minMoveThreshold)
            {
                PlayIdle();
                return;
            }

            // Transition to running
            TransitionToState(AnimationState.Running);

            // Update direction
            UpdateDirection(direction);
            UpdateDirectionParameters(direction);
            UpdateSpriteFlip(direction);
        }

        // Set movement speed (nếu cần điều chỉnh animation speed)
        public void SetMovementSpeed(float speed)
        {
            if (settings != null)
            {
                float animSpeed = settings.GetAnimationSpeed(speed);
                animator.speed = animSpeed;
            }
        }

        // Stop movement và return to idle
        public void StopMovement()
        {
            PlayIdle();
        }

        // TOOL ANIMATIONS

        // Play Hoeing animation (cuốc đất)
        public bool PlayHoeing()
        {
            return PlayToolAnimation(ToolType.Hoe);
        }

        // Play Watering animation (tưới nước)
        public bool PlayWatering()
        {
            return PlayToolAnimation(ToolType.Watering);
        }

        // Play Sickle animation (liềm)
        public bool PlaySickle()
        {
            return PlayToolAnimation(ToolType.Sickle);
        }

        /// Play PickUp animation - tự động chọn Idle hoặc Run version
        public bool PlayPickUp()
        {
            // Determine which pickup animation to use
            ToolType pickUpType = (currentState == AnimationState.Running)
                ? ToolType.PickUpRun
                : ToolType.PickUpIdle;

            return PlayToolAnimation(pickUpType);
        }

        /// Play PickUp Idle version
        public bool PlayPickUpIdle()
        {
            return PlayToolAnimation(ToolType.PickUpIdle);
        }

        /// Play PickUp Run version
        public bool PlayPickUpRun()
        {
            return PlayToolAnimation(ToolType.PickUpRun);
        }

        /// Play Sleep animation
        public void PlaySleep()
        {
            if (isActionLocked)
            {
                LogDebug("Cannot sleep - action locked");
                return;
            }

            TransitionToState(AnimationState.Sleep);

            // Sleep thường là hướng Down
            lastDirectionVector = Vector2.down;
            UpdateDirectionParameters(lastDirectionVector);
        }

        /// Wake up từ sleep
        public void WakeUp()
        {
            if (currentState == AnimationState.Sleep)
            {
                PlayIdle();
            }
        }

        // CORE TOOL ANIMATION SYSTEM

        private bool PlayToolAnimation(ToolType toolType)
        {
            LogDebug($"🔧 PlayToolAnimation: {toolType}");

            // Validate
            if (isActionLocked)
            {
                LogDebug($"⏳ Cannot play {toolType} - action locked");
                return false;
            }

            if (toolMapper == null)
            {
                Debug.LogError("ToolAnimationMapper is null!");
                return false;
            }

            // Get animation state từ ToolMapper
            AnimationState toolState = toolMapper.GetAnimationState(toolType);
            if (toolState == AnimationState.Idle)
            {
                Debug.LogWarning($"No animation state mapped for tool: {toolType}");
                return false;
            }

            // Get config
            ToolAnimationConfig config = toolMapper.GetConfig(toolType);
            if (config == null)
            {
                Debug.LogError($"No config found for tool: {toolType}");
                return false;
            }

            // Get duration
            float duration = config.GetDuration(currentDirection);
            if (duration <= 0f)
            {
                Debug.LogWarning($"Invalid duration for {toolType}: {duration}");
                duration = 1f; // Fallback
            }

            LogDebug($"✅ Playing tool animation: {toolState}, duration: {duration:F2}s");

            // Start animation
            StartToolAction(toolState, duration);

            return true;
        }

        private void StartToolAction(AnimationState toolState, float duration)
        {
            LogDebug($"StartToolAction: {toolState}, duration: {duration:F2}s");

            // Stop previous action if any
            if (actionCoroutine != null)
            {
                StopCoroutine(actionCoroutine);
                actionCoroutine = null;
            }

            // Store previous state for return
            AnimationState previousMovementState = currentState;
            if (IsToolAction(previousMovementState))
            {
                // If already in tool state, use Idle as fallback
                previousMovementState = AnimationState.Idle;
            }

            // Transition to tool state
            TransitionToState(toolState);

            // Keep current direction
            UpdateDirectionParameters(lastDirectionVector);

            // Start action lock coroutine
            actionCoroutine = StartCoroutine(ActionLockCoroutine(duration, previousMovementState));

            // Fire event
            OnToolActionStarted?.Invoke(toolState);
        }

        private IEnumerator ActionLockCoroutine(float duration, AnimationState returnState)
        {
            // Lock
            isActionLocked = true;
            actionStartTime = Time.time;
            actionDuration = duration;

            LogDebug($"Action locked for {duration:F2}s, will return to: {returnState}");

            // Wait for duration
            yield return new WaitForSeconds(duration);

            // Unlock
            isActionLocked = false;
            actionDuration = 0f;

            LogDebug($"Action unlocked, returning to: {returnState}");

            // Fire complete event
            OnActionComplete?.Invoke();

            // Return to previous state (Idle or Running)
            TransitionToState(returnState);
            UpdateDirectionParameters(lastDirectionVector);

            actionCoroutine = null;
        }

        // STATE MANAGEMENT

        private void TransitionToState(AnimationState newState)
        {
            // Skip if same state
            if (currentState == newState)
                return;

            // Validate transition
            if (!stateValidator.CanTransition(currentState, newState))
            {
                LogDebug($"Invalid transition: {currentState} → {newState}");
                return;
            }

            // Perform transition
            previousState = currentState;
            currentState = newState;

            // Update animator
            SetAnimatorState(newState);

            // Fire event
            OnStateChanged?.Invoke(newState);

            LogDebug($"State: {previousState} → {currentState}");
        }

        private void SetAnimatorState(AnimationState state)
        {
            int stateValue = (int)state;
            SetAnimatorInt(PARAM_STATE, stateValue);
        }

        public void SetCurrentTool(ToolType toolType)
        {
            currentToolType = toolType;
            LogDebug($"SetCurrentTool: {toolType}");
        }

        public ToolType GetCurrentTool()
        {
            return currentToolType;
        }

        // DIRECTION HANDLING

        private void UpdateDirection(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude < minMoveThreshold)
                return;

            // Save direction vector (normalized)
            lastDirectionVector = moveInput.normalized;

            // Determine Direction enum based on dominant axis
            float absX = Mathf.Abs(moveInput.x);
            float absY = Mathf.Abs(moveInput.y);

            if (absY > absX)
            {
                // Vertical dominant
                currentDirection = moveInput.y > 0 ? Direction.Up : Direction.Down;
            }
            else
            {
                // Horizontal dominant
                currentDirection = Direction.Side;
            }

            LogDebug($"UpdateDirection: input={moveInput:F2}, dir={currentDirection}, absX={absX:F2}, absY={absY:F2}");
        }

        private void UpdateDirectionParameters(Vector2 direction)
        {
            // Normalize if needed
            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            // Set animator parameters
            SetAnimatorFloat(PARAM_HORIZONTAL, direction.x);
            SetAnimatorFloat(PARAM_VERTICAL, direction.y);
        }

        private void UpdateSpriteFlip(Vector2 direction)
        {
            // Only flip for horizontal movement
            if (currentDirection == Direction.Side)
            {
                bool shouldFlipLeft = direction.x < 0;
                spriteRenderer.flipX = shouldFlipLeft;
                LogDebug($"Sprite flip: {shouldFlipLeft}");
            }
        }

        // Manually set sprite flip (for external use)
        public void SetSpriteFlip(bool flipX)
        {
            spriteRenderer.flipX = flipX;
        }

        // ANIMATOR CONTROL (sử dụng ParameterCache)

        private void SetAnimatorFloat(string paramName, float value)
        {
            if (paramCache.HasParameter(paramName))
            {
                int hash = paramCache.GetHash(paramName);
                animator.SetFloat(hash, value);
            }
            else
            {
                Debug.LogWarning($"Parameter not found: {paramName}");
            }
        }

        private void SetAnimatorInt(string paramName, int value)
        {
            if (paramCache.HasParameter(paramName))
            {
                int hash = paramCache.GetHash(paramName);
                animator.SetInteger(hash, value);
            }
            else
            {
                Debug.LogWarning($"Parameter not found: {paramName}");
            }
        }

        /// Called từ Animation Event khi tool impact (cuốc chạm đất, etc.)
        public void OnToolImpact()
        {
            LogDebug($"Tool impact: {currentState}");
            // Fire event cho gameplay logic (spawn particles, apply effect, etc.)
        }

        /// Called từ Animation Event khi có footstep
        public void OnFootstep()
        {
            LogDebug("Footstep");
            // Play footstep sound
        }

        /// Called từ Animation Event khi animation bắt đầu
        public void OnAnimationStart(string animName)
        {
            LogDebug($"Animation started: {animName}");
        }

        /// Called từ Animation Event khi animation kết thúc
        public void OnAnimationComplete(string animName)
        {
            LogDebug($"Animation complete: {animName}");
        }

        /// Force stop animation và return về Idle
        public void ForceStop()
        {
            // Stop coroutine
            if (actionCoroutine != null)
            {
                StopCoroutine(actionCoroutine);
                actionCoroutine = null;
            }

            // Unlock
            isActionLocked = false;
            actionDuration = 0f;

            // Return to idle
            PlayIdle();

            LogDebug("Force stopped");
        }

        /// Check if có thể perform action
        public bool CanPerformAction()
        {
            return !isActionLocked;
        }

        /// Get progress của action hiện tại (0-1)
        public float GetActionProgress()
        {
            if (!isActionLocked || actionDuration <= 0f)
                return 1f;

            float elapsed = Time.time - actionStartTime;
            return Mathf.Clamp01(elapsed / actionDuration);
        }

        /// Check if hiện tại đang trong tool action state
        public bool IsToolAction(AnimationState state)
        {
            return stateValidator.IsToolAction(state);
        }

        /// Get current state name
        public string GetCurrentStateName()
        {
            return currentState.ToString();
        }

        // DEBUG
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[PlayerAnim] {message}");
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }

        [ContextMenu("Test - Play Idle")]
        private void TestIdle() => PlayIdle();

        [ContextMenu("Test - Play Hoeing")]
        private void TestHoeing() => PlayHoeing();

        [ContextMenu("Test - Play Watering")]
        private void TestWatering() => PlayWatering();

        [ContextMenu("Test - Play Sickle")]
        private void TestSickle() => PlaySickle();

        [ContextMenu("Test - Play PickUp")]
        private void TestPickUp() => PlayPickUp();

        [ContextMenu("Test - Play Sleep")]
        private void TestSleep() => PlaySleep();

        [ContextMenu("Test - Force Stop")]
        private void TestForceStop() => ForceStop();

        [ContextMenu("Debug - Log State")]
        private void DebugLogState()
        {
            Debug.Log("=== ANIMATION STATE ===");
            Debug.Log($"State: {currentState} (prev: {previousState})");
            Debug.Log($"Direction: {currentDirection}");
            Debug.Log($"Direction Vector: {lastDirectionVector}");
            Debug.Log($"Action Locked: {isActionLocked}");
            if (isActionLocked)
                Debug.Log($"Action Progress: {GetActionProgress():P0}");
            Debug.Log($"Sprite Flip: {spriteRenderer.flipX}");
        }

        [ContextMenu("Debug - List Parameters")]
        private void DebugListParameters()
        {
            if (paramCache == null)
            {
                Debug.Log("ParameterCache not initialized");
                return;
            }

            Debug.Log("=== ANIMATOR PARAMETERS ===");
            Debug.Log($"State: {animator.GetInteger(paramCache.GetHash(PARAM_STATE))}");
            Debug.Log($"Horizontal: {animator.GetFloat(paramCache.GetHash(PARAM_HORIZONTAL))}");
            Debug.Log($"Vertical: {animator.GetFloat(paramCache.GetHash(PARAM_VERTICAL))}");
        }
#endif
    }
}

