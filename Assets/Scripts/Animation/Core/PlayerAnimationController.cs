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
        [SerializeField] private ToolAnimationConfig[] toolConfigs;

        [Header("Settings")]
        [SerializeField] private AnimationSettings settings;
        [SerializeField] private bool debugMode = false;

        [Header("Runtime Info")]
        [SerializeField, ReadOnly] private AnimationState currentState = AnimationState.Idle;
        [SerializeField, ReadOnly] private AnimationState previousState = AnimationState.Idle;
        [SerializeField, ReadOnly] private Direction currentDirection = Direction.Down;
        [SerializeField, ReadOnly] private bool isActionLocked = false;
        [SerializeField, ReadOnly] private float actionLockTimer = 0f;

        // Private components
        private AnimationParameterCache paramCache;
        private AnimationStateValidator stateValidator;
        private ToolAnimationMapper toolMapper;
        private ToolType currentToolType = ToolType.None;
        private Coroutine currentActionCoroutine;

        // Animator parameter names (constants)
        private const string PARAM_IS_MOVING = "isMoving";
        private const string PARAM_HORIZONTAL = "Horizontal";
        private const string PARAM_VERTICAL = "Vertical";

        // Events
        public event Action<AnimationState> OnStateChanged;
        public event Action<string> OnAnimationEvent;
        public event Action OnActionComplete;
        public event Action<ToolType, Direction> OnToolActionStarted;
        public event Action<ToolType, Direction> OnToolActionImpact;

        // PROPERTIES
        public AnimationState CurrentState => currentState;
        public AnimationState PreviousState => previousState;
        public Direction CurrentDirection => currentDirection;
        public bool IsActionLocked => isActionLocked;
        public ToolType CurrentTool => currentToolType;
        public Animator Animator => animator;

        private void Awake()
        {
            // Get components
            if (animator == null)
                animator = GetComponent<Animator>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();

            // Initialize helper classes
            paramCache = new AnimationParameterCache();
            stateValidator = new AnimationStateValidator();
            toolMapper = new ToolAnimationMapper();
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdateActionLock();
        }

        private void OnDestroy()
        {
            // Stop all coroutines
            if (currentActionCoroutine != null)
            {
                StopCoroutine(currentActionCoroutine);
                currentActionCoroutine = null;
            }
        }

        private void Initialize()
        {
            SetupComponents();
            LoadConfigurations();

            // Set initial state
            currentState = AnimationState.Idle;
            PlayIdleAnimation();

            LogDebug("PlayerAnimationController initialized");
        }

        private void SetupComponents()
        {
            // Initialize parameter cache
            paramCache.Initialize(animator);

            // Setup state validator
            stateValidator.SetupPriorities();

            // Validate animator
            if (animator == null)
            {
                Debug.LogError("PlayerAnimationController: Animator is missing!");
                enabled = false;
                return;
            }

            if (animator.runtimeAnimatorController == null)
            {
                Debug.LogError("PlayerAnimationController: Animator Controller is not assigned!");
                enabled = false;
                return;
            }
        }

        private void LoadConfigurations()
        {
            // Load tool animation configs
            toolMapper.LoadConfigs(toolConfigs);

            // Validate settings
            if (settings == null)
            {
                Debug.LogWarning("PlayerAnimationController: AnimationSettings is not assigned! Using defaults.");
                settings = ScriptableObject.CreateInstance<AnimationSettings>();
            }

            LogDebug($"Loaded {toolConfigs?.Length ?? 0} tool configurations");
        }

        // STATE MANAGEMENT
        public AnimationState GetCurrentState()
        {
            return currentState;
        }

        public AnimationState GetPreviousState()
        {
            return previousState;
        }

        public bool IsInState(AnimationState state)
        {
            return currentState == state;
        }

        public bool IsActionState()
        {
            return stateValidator.IsActionState(currentState);
        }

        public void SetCurrentTool(ToolType tool)
        {
            currentToolType = tool;
            LogDebug($"Current tool set to: {tool}");
        }

        // MOVEMENT ANIMATIONS
        public void PlayIdleAnimation()
        {
            if (isActionLocked)
                return;

            SetAnimatorBool(PARAM_IS_MOVING, false);
            TransitionToState(AnimationState.Idle);
        }

        public void PlayWalkingAnimation(Vector2 direction)
        {
            if (isActionLocked)
                return;

            // Update direction based on movement
            UpdateDirection(direction);

            // Set animator parameters
            SetAnimatorBool(PARAM_IS_MOVING, true);
            SetAnimatorFloat(PARAM_HORIZONTAL, direction.x);
            SetAnimatorFloat(PARAM_VERTICAL, direction.y);

            // Update sprite flip for horizontal movement
            UpdateSpriteFlip(direction);

            TransitionToState(AnimationState.Running);
        }

        public void PlayRunningAnimation(Vector2 direction)
        {
            PlayWalkingAnimation(direction); // Same as walking in this setup
        }

        public void SetMovementSpeed(float speed)
        {
            if (settings.ShouldPlayIdle(speed))
            {
                PlayIdleAnimation();
            }
            else
            {
                // Continue with current direction
                Vector2 dir = GetDirectionVector();
                PlayWalkingAnimation(dir);
            }
        }

        public void StopMovement()
        {
            PlayIdleAnimation();
        }

        private void UpdateMovementAnimation(float speed)
        {
            float animSpeed = settings.GetAnimationSpeed(speed);
            animator.speed = animSpeed;
        }

        // TOOL ANIMATIONS
        // Play tool animation based on current tool and direction
        public bool PlayToolAnimation(ToolType tool)
        {
            return PlayToolAnimation(tool, currentDirection);
        }

        // Play tool animation with specific direction
        public bool PlayToolAnimation(ToolType tool, Direction dir)
        {
            // Validate
            if (isActionLocked)
            {
                LogDebug($"Cannot play tool animation - action is locked");
                return false;
            }

            if (!toolMapper.CanPlayAnimation(tool))
            {
                LogDebug($"Cannot play animation for tool: {tool}");
                return false;
            }

            if (!toolMapper.HasClipForDirection(tool, dir))
            {
                Debug.LogWarning($"No animation clip for {tool} in direction {dir}");
                return false;
            }

            // Get config
            var config = toolMapper.GetConfig(tool);
            if (config == null)
            {
                Debug.LogError($"No config found for tool: {tool}");
                return false;
            }

            // Start animation coroutine
            if (currentActionCoroutine != null)
            {
                StopCoroutine(currentActionCoroutine);
            }

            currentActionCoroutine = StartCoroutine(PlayToolActionCoroutine(tool, dir, config));
            return true;
        }

        // Specific tool animation methods
        public bool PlayWateringAnimation()
        {
            return PlayToolAnimation(ToolType.Watering, currentDirection);
        }

        public bool PlayHoeingAnimation()
        {
            return PlayToolAnimation(ToolType.Hoe, currentDirection);
        }

        public bool PlayPlantingAnimation()
        {
            return PlayToolAnimation(ToolType.Seeds, currentDirection);
        }

        public bool PlayHarvestingAnimation()
        {
            return PlayToolAnimation(ToolType.Sickle, currentDirection);
        }

        public bool PlayPickaxeAnimation()
        {
            return PlayToolAnimation(ToolType.PickUp, currentDirection);
        }

        public bool PlayCarryingAnimation()
        {
            return PlayToolAnimation(ToolType.Seeds, currentDirection);
        }

        // TOOL ACTION COROUTINE
        private IEnumerator PlayToolActionCoroutine(ToolType tool, Direction dir, ToolAnimationConfig config)
        {
            // Get animation state
            AnimationState toolState = toolMapper.GetAnimationState(tool);

            // Transition to tool state
            if (!TransitionToState(toolState))
            {
                LogDebug($"Failed to transition to state: {toolState}");
                yield break;
            }

            // Get animator trigger
            string trigger = toolMapper.GetAnimationTrigger(tool, dir);

            // Set trigger
            SetAnimatorTrigger(trigger);

            // Lock animation
            float duration = config.GetAnimationDuration(dir);
            LockActionAnimation(duration);

            // Fire event
            OnToolActionStarted?.Invoke(tool, dir);
            LogDebug($"Started tool action: {tool} {dir} (duration: {duration:F2}s)");

            // Wait for impact frame
            float impactTime = config.GetImpactTime(dir);
            yield return new WaitForSeconds(impactTime);

            // Fire impact event
            OnToolActionImpact?.Invoke(tool, dir);
            OnAnimationEvent?.Invoke($"Impact_{tool}_{dir}");
            LogDebug($"Tool impact: {tool} {dir}");

            // Wait for animation to complete
            float remainingTime = duration - impactTime;
            yield return new WaitForSeconds(remainingTime);

            // Animation complete
            OnAnimationCompleteCallback(tool.ToString());

            currentActionCoroutine = null;
        }

        // TRANSITION SYSTEM
        private bool TransitionToState(AnimationState newState)
        {
            if (!CanTransitionTo(newState))
            {
                LogDebug($"Cannot transition from {currentState} to {newState}");
                return false;
            }

            PerformTransition(newState);
            return true;
        }

        private bool CanTransitionTo(AnimationState newState)
        {
            // Same state - no transition needed
            if (currentState == newState)
                return false;

            // Validate with state validator
            return stateValidator.ValidateTransition(currentState, newState);
        }

        private void PerformTransition(AnimationState newState)
        {
            previousState = currentState;
            currentState = newState;

            OnTransitionComplete(newState);

            LogDebug($"State transition: {previousState} → {currentState}");
        }

        private void OnTransitionComplete(AnimationState newState)
        {
            // Fire event
            OnStateChanged?.Invoke(newState);
        }

        // ANIMATOR CONTROL
        private void SetAnimatorParameter(string paramName, object value)
        {
            if (!paramCache.HasParameter(paramName))
            {
                Debug.LogWarning($"Parameter '{paramName}' not found in Animator");
                return;
            }

            int hash = paramCache.GetHash(paramName);

            switch (value)
            {
                case bool b:
                    animator.SetBool(hash, b);
                    break;
                case float f:
                    animator.SetFloat(hash, f);
                    break;
                case int i:
                    animator.SetInteger(hash, i);
                    break;
            }
        }

        private void SetAnimatorTrigger(string triggerName)
        {
            if (!paramCache.HasParameter(triggerName))
            {
                Debug.LogWarning($"Trigger '{triggerName}' not found in Animator");
                return;
            }

            int hash = paramCache.GetHash(triggerName);
            animator.SetTrigger(hash);

            LogDebug($"Set trigger: {triggerName}");
        }

        private void SetAnimatorFloat(string paramName, float value)
        {
            SetAnimatorParameter(paramName, value);
        }

        private void SetAnimatorBool(string paramName, bool value)
        {
            SetAnimatorParameter(paramName, value);
        }

        private void SetAnimatorInt(string paramName, int value)
        {
            SetAnimatorParameter(paramName, value);
        }

        // ACTION LOCKING
        private void LockActionAnimation(float duration)
        {
            isActionLocked = true;
            actionLockTimer = duration;
            LogDebug($"Action locked for {duration:F2}s");
        }

        private void UnlockActionAnimation()
        {
            isActionLocked = false;
            actionLockTimer = 0f;
            LogDebug("Action unlocked");
        }

        private void UpdateActionLock()
        {
            if (isActionLocked)
            {
                actionLockTimer -= Time.deltaTime;
                if (actionLockTimer <= 0f)
                {
                    UnlockActionAnimation();
                }
            }
        }

        public bool IsAnimationLocked()
        {
            return isActionLocked;
        }

        // ANIMATION CALLBACKS


        // Called by AnimationEventHandler when animation starts
        public void OnAnimationStart(string animName)
        {
            OnAnimationEvent?.Invoke($"Start_{animName}");
            LogDebug($"Animation started: {animName}");
        }

        /// Called by AnimationEventHandler when animation completes
        public void OnAnimationComplete(string animName)
        {
            OnAnimationCompleteCallback(animName);
        }

        private void OnAnimationCompleteCallback(string animName)
        {
            OnAnimationEvent?.Invoke($"Complete_{animName}");
            OnActionComplete?.Invoke();

            // Return to idle if in action state
            if (IsActionState())
            {
                ResetToIdle();
            }

            LogDebug($"Animation complete: {animName}");
        }

        // Called by AnimationEventHandler on impact frame
        public void OnActionImpact(string actionType)
        {
            OnAnimationEvent?.Invoke($"Impact_{actionType}");
            LogDebug($"Action impact: {actionType}");
        }

        /// Called by AnimationEventHandler on footstep
        public void OnFootstep()
        {
            OnAnimationEvent?.Invoke("Footstep");
        }

        // DIRECTION HANDLING
        private void UpdateDirection(Vector2 moveInput)
        {
            if (moveInput.sqrMagnitude < 0.01f)
                return;

            // Determine direction based on input
            if (Mathf.Abs(moveInput.y) > Mathf.Abs(moveInput.x))
            {
                // Vertical movement
                currentDirection = moveInput.y > 0 ? Direction.Up : Direction.Down;
            }
            else
            {
                // Horizontal movement
                currentDirection = Direction.Side;
            }
        }

        private void UpdateSpriteFlip(Vector2 direction)
        {
            if (currentDirection == Direction.Side)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }

        private Vector2 GetDirectionVector()
        {
            return currentDirection switch
            {
                Direction.Down => Vector2.down,
                Direction.Up => Vector2.up,
                Direction.Side => spriteRenderer.flipX ? Vector2.left : Vector2.right,
                _ => Vector2.down
            };
        }

        // UTILITY
        public void ForceStopAnimation()
        {
            if (currentActionCoroutine != null)
            {
                StopCoroutine(currentActionCoroutine);
                currentActionCoroutine = null;
            }

            UnlockActionAnimation();
            ResetToIdle();

            LogDebug("Animation force stopped");
        }

        public void ResetToIdle()
        {
            currentState = AnimationState.Idle;
            PlayIdleAnimation();
        }

        public float GetAnimationProgress()
        {
            if (!isActionLocked || actionLockTimer <= 0f)
                return 1f;

            var config = toolMapper.GetConfig(currentToolType);
            if (config == null)
                return 0f;

            float duration = config.GetAnimationDuration(currentDirection);
            return 1f - (actionLockTimer / duration);
        }

        public bool CanPerformAction()
        {
            return !isActionLocked && !IsActionState();
        }

        // DEBUG
        private void LogDebug(string message)
        {
            if (debugMode)
            {
                Debug.Log($"[PlayerAnimController] {message}");
            }
        }

        private void LogStateChange(AnimationState from, AnimationState to)
        {
            if (debugMode)
            {
                Debug.Log($"[PlayerAnimController] State: {from} → {to}");
            }
        }

        private void LogAnimationEvent(string eventName)
        {
            if (debugMode)
            {
                Debug.Log($"[PlayerAnimController] Event: {eventName}");
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Test Hoe Down")]
        private void TestHoeDown()
        {
            PlayToolAnimation(ToolType.Hoe, Direction.Down);
        }

        [ContextMenu("Test Watering Up")]
        private void TestWateringUp()
        {
            PlayToolAnimation(ToolType.Watering, Direction.Up);
        }

        [ContextMenu("Test Sickle Side")]
        private void TestSickleSide()
        {
            PlayToolAnimation(ToolType.Sickle, Direction.Side);
        }

        [ContextMenu("Force Stop")]
        private void TestForceStop()
        {
            ForceStopAnimation();
        }

        [ContextMenu("Log State")]
        private void TestLogState()
        {
            Debug.Log($"Current State: {currentState}");
            Debug.Log($"Previous State: {previousState}");
            Debug.Log($"Direction: {currentDirection}");
            Debug.Log($"Action Locked: {isActionLocked}");
            Debug.Log($"Current Tool: {currentToolType}");
        }

        private void OnValidate()
        {
            if (animator == null)
                animator = GetComponent<Animator>();

            if (spriteRenderer == null)
                spriteRenderer = GetComponent<SpriteRenderer>();
        }
#endif
    }
}

