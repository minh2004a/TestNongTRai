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
        public Vector2 CurrentDirectionVector => lastDirectionVector;


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
            paramCache = new AnimationParameterCache();
            paramCache.Initialize(animator);

            stateValidator = new AnimationStateValidator();
            stateValidator.Initialize();

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

            SetAnimatorState(AnimationState.Idle);
            UpdateDirectionParameters(lastDirectionVector);

            LogDebug("PlayerAnimationController initialized");
        }

        // Exit pickup state and return to normal movement state
        // Call this when item is deselected/dropped
        public bool ExitPickUpState()
        {
            if (isActionLocked)
            {
                LogDebug("Cannot exit PickUp - action locked");
                return false;
            }

            // Only exit if currently in pickup state
            if (currentState != AnimationState.PickUpIdle &&
                currentState != AnimationState.PickUpRun)
            {
                LogDebug("Not in PickUp state, no need to exit");
                return false;
            }

            // Return to appropriate state based on current pickup state
            if (currentState == AnimationState.PickUpRun)
            {
                TransitionToState(AnimationState.Running);
            }
            else
            {
                TransitionToState(AnimationState.Idle);
            }

            UpdateDirectionParameters(lastDirectionVector);
            LogDebug("✅ Exited PickUp state");
            return true;
        }

        // Check if currently in any pickup state
        public bool IsInPickUpState()
        {
            return currentState == AnimationState.PickUpIdle ||
                   currentState == AnimationState.PickUpRun;
        }

        // ==========================================
        // MOVEMENT ANIMATIONS
        // ==========================================

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

        public void PlayMovement(Vector2 direction)
        {
            if (isActionLocked)
            {
                LogDebug("Cannot play movement - action locked");
                return;
            }

            if (direction.sqrMagnitude < minMoveThreshold)
            {
                PlayIdle();
                return;
            }

            TransitionToState(AnimationState.Running);
            UpdateDirection(direction);
            UpdateDirectionParameters(direction);
            UpdateSpriteFlip(direction);
        }

        public void SetMovementSpeed(float speed)
        {
            if (settings != null)
            {
                float animSpeed = settings.GetAnimationSpeed(speed);
                animator.speed = animSpeed;
            }
        }

        public void StopMovement()
        {
            PlayIdle();
        }

        // ==========================================
        // TOOL ANIMATIONS
        // ==========================================

        public bool PlayHoeing()
        {
            return PlayToolAnimation(ToolType.Hoe);
        }

        public bool PlayWatering()
        {
            return PlayToolAnimation(ToolType.Watering);
        }

        public bool PlaySickle()
        {
            return PlayToolAnimation(ToolType.Sickle);
        }

        // ==========================================
        // PICKUP ANIMATIONS (Visual States - NO LOCK)
        // ==========================================

        // Play PickUpIdle animation - Visual state only, no action lock
        public bool PlayPickUpIdle()
        {
            if (isActionLocked)
            {
                LogDebug("Cannot play PickUpIdle - action locked");
                return false;
            }

            TransitionToState(AnimationState.PickUpIdle);
            UpdateDirectionParameters(lastDirectionVector);

            LogDebug("Playing PickUpIdle (visual state)");
            return true;
        }

        /// Play PickUpRun animation - Visual state only, no action lock
        public bool PlayPickUpRun()
        {
            if (isActionLocked)
            {
                LogDebug("Cannot play PickUpRun - action locked");
                return false;
            }

            TransitionToState(AnimationState.PickUpRun);
            UpdateDirectionParameters(lastDirectionVector);

            LogDebug("Playing PickUpRun (visual state)");
            return true;
        }

        /// Play PickUp animation - Auto select Idle or Run based on current state
        public bool PlayPickUp()
        {
            // Determine which pickup to use based on current state
            bool isRunning = (currentState == AnimationState.Running || currentState == AnimationState.PickUpRun);

            if (isRunning)
                return PlayPickUpRun();
            else
                return PlayPickUpIdle();
        }

        // ==========================================
        // SLEEP ANIMATIONS
        // ==========================================

        public void PlaySleep()
        {
            if (isActionLocked)
            {
                LogDebug("Cannot sleep - action locked");
                return;
            }

            TransitionToState(AnimationState.Sleep);
            lastDirectionVector = Vector2.down;
            UpdateDirectionParameters(lastDirectionVector);
        }

        public void WakeUp()
        {
            if (currentState == AnimationState.Sleep)
            {
                PlayIdle();
            }
        }

        // ==========================================
        // CORE TOOL ANIMATION SYSTEM
        // ==========================================

        private bool PlayToolAnimation(ToolType toolType)
        {
            LogDebug($"🔧 PlayToolAnimation: {toolType}");

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

            AnimationState toolState = toolMapper.GetAnimationState(toolType);
            if (toolState == AnimationState.Idle)
            {
                Debug.LogWarning($"No animation state mapped for tool: {toolType}");
                return false;
            }

            ToolAnimationConfig config = toolMapper.GetConfig(toolType);
            if (config == null)
            {
                Debug.LogError($"No config found for tool: {toolType}");
                return false;
            }

            float duration = config.GetDuration(currentDirection);
            if (duration <= 0f)
            {
                Debug.LogWarning($"Invalid duration for {toolType}: {duration}");
                duration = 1f;
            }

            LogDebug($"✅ Playing tool animation: {toolState}, duration: {duration:F2}s");
            StartToolAction(toolState, duration);

            return true;
        }

        private void StartToolAction(AnimationState toolState, float duration)
        {
            LogDebug($"StartToolAction: {toolState}, duration: {duration:F2}s");

            if (actionCoroutine != null)
            {
                StopCoroutine(actionCoroutine);
                actionCoroutine = null;
            }

            AnimationState previousMovementState = currentState;
            if (IsToolAction(previousMovementState))
            {
                previousMovementState = AnimationState.Idle;
            }

            TransitionToState(toolState);
            UpdateDirectionParameters(lastDirectionVector);

            actionCoroutine = StartCoroutine(ActionLockCoroutine(duration, previousMovementState));
            OnToolActionStarted?.Invoke(toolState);
        }

        private IEnumerator ActionLockCoroutine(float duration, AnimationState returnState)
        {
            isActionLocked = true;
            actionStartTime = Time.time;
            actionDuration = duration;

            LogDebug($"Action locked for {duration:F2}s, will return to: {returnState}");

            yield return new WaitForSeconds(duration);

            isActionLocked = false;
            actionDuration = 0f;

            LogDebug($"Action unlocked, returning to: {returnState}");

            OnActionComplete?.Invoke();
            TransitionToState(returnState);
            UpdateDirectionParameters(lastDirectionVector);

            actionCoroutine = null;
        }

        // ==========================================
        // STATE MANAGEMENT
        // ==========================================

        private void TransitionToState(AnimationState newState)
        {
            if (currentState == newState)
                return;

            if (!stateValidator.CanTransition(currentState, newState))
            {
                LogDebug($"Invalid transition: {currentState} → {newState}");
                return;
            }

            previousState = currentState;
            currentState = newState;

            SetAnimatorState(newState);
            OnStateChanged?.Invoke(newState);

            LogDebug($"State: {previousState} → {currentState}");
        }

        private void SetAnimatorState(AnimationState state)
        {
            int stateValue = (int)state;
            Debug.Log($"[PlayerAnim] 🎯 SetAnimatorState: {state} = {stateValue}");
            SetAnimatorInt(PARAM_STATE, stateValue);

            // ✅ THÊM: Verify animator parameter was set
            if (animator != null && paramCache != null)
            {
                int hash = paramCache.GetHash(PARAM_STATE);
                int actualValue = animator.GetInteger(hash);
                Debug.Log($"[PlayerAnim] Animator State parameter is now: {actualValue}");
            }
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

        // ==========================================
        // DIRECTION HANDLING
        // ==========================================

        public void UpdateDirection(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude < minMoveThreshold)
                return;

            lastDirectionVector = moveInput.normalized;

            float absX = Mathf.Abs(moveInput.x);
            float absY = Mathf.Abs(moveInput.y);

            if (absY > absX)
            {
                currentDirection = moveInput.y > 0 ? Direction.Up : Direction.Down;
            }
            else
            {
                currentDirection = Direction.Side;
            }

            LogDebug($"UpdateDirection: input={moveInput:F2}, dir={currentDirection}");
        }

        public void UpdateDirectionParameters(Vector2 direction)
        {
            if (direction.sqrMagnitude > 1f)
                direction.Normalize();

            SetAnimatorFloat(PARAM_HORIZONTAL, direction.x);
            SetAnimatorFloat(PARAM_VERTICAL, direction.y);
        }

        public void UpdateSpriteFlip(Vector2 direction)
        {
            if (currentDirection == Direction.Side)
            {
                bool shouldFlipLeft = direction.x < 0;
                spriteRenderer.flipX = shouldFlipLeft;
                LogDebug($"Sprite flip: {shouldFlipLeft}");
            }
        }

        public void SetSpriteFlip(bool flipX)
        {
            spriteRenderer.flipX = flipX;
        }

        // ==========================================
        // ANIMATOR CONTROL
        // ==========================================

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

        // ==========================================
        // ANIMATION EVENTS
        // ==========================================

        public void OnToolImpact()
        {
            LogDebug($"Tool impact: {currentState}");
        }

        public void OnFootstep()
        {
            LogDebug("Footstep");
        }

        public void OnAnimationStart(string animName)
        {
            LogDebug($"Animation started: {animName}");
        }

        public void OnAnimationComplete(string animName)
        {
            LogDebug($"Animation complete: {animName}");
        }

        // ==========================================
        // PUBLIC UTILITIES
        // ==========================================

        public void ForceStop()
        {
            if (actionCoroutine != null)
            {
                StopCoroutine(actionCoroutine);
                actionCoroutine = null;
            }

            isActionLocked = false;
            actionDuration = 0f;
            PlayIdle();

            LogDebug("Force stopped");
        }

        public bool CanPerformAction()
        {
            return !isActionLocked;
        }

        public float GetActionProgress()
        {
            if (!isActionLocked || actionDuration <= 0f)
                return 1f;

            float elapsed = Time.time - actionStartTime;
            return Mathf.Clamp01(elapsed / actionDuration);
        }

        public bool IsToolAction(AnimationState state)
        {
            return stateValidator.IsToolAction(state);
        }

        public string GetCurrentStateName()
        {
            return currentState.ToString();
        }

        // ==========================================
        // DEBUG
        // ==========================================

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

        [ContextMenu("Test - Play PickUpIdle")]
        private void TestPickUpIdle() => PlayPickUpIdle();

        [ContextMenu("Test - Play PickUpRun")]
        private void TestPickUpRun() => PlayPickUpRun();

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

