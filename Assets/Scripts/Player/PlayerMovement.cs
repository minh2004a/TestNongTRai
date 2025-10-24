using System.Collections;
using System.Collections.Generic;
using TinyFarm.Animation;
using TinyFarm.Items;
using UnityEngine;

[RequireComponent(typeof(PlayerAnimationController))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAnimationController animController;
    [SerializeField] private Rigidbody2D rb;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeedMultiplier = 1.5f;

    [Header("Input Settings")]
    [SerializeField] private bool useRawInput = true;
    [SerializeField] private KeyCode runKey = KeyCode.LeftShift;

    [Header("Tool Keys")]
    [SerializeField] private KeyCode hoeKey = KeyCode.Alpha1;
    [SerializeField] private KeyCode wateringKey = KeyCode.Alpha2;
    [SerializeField] private KeyCode sickleKey = KeyCode.Alpha3;
    [SerializeField] private KeyCode pickUpKey = KeyCode.E;
    [SerializeField] private KeyCode sleepKey = KeyCode.R;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color debugTextColor = Color.white;

    // Runtime
    private Vector2 moveInput;
    private Vector2 normalizedInput;
    private Vector2 lastNonZeroInput;
    private bool isRunning;
    private float currentSpeed;

    // Debug tracking
    private string lastInputString = "";
    private string lastDirectionString = "";

    // ==========================================
    // UNITY LIFECYCLE
    // ==========================================

    private void Awake()
    {
        InitializeComponents();
    }

    private void Start()
    {
        lastNonZeroInput = Vector2.down;
        LogDebug("PlayerMovement initialized");
    }

    private void Update()
    {
        HandleInput();
        HandleToolInput();
        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void OnValidate()
    {
        if (animController == null)
            animController = GetComponent<PlayerAnimationController>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    // ==========================================
    // INITIALIZATION
    // ==========================================

    private void InitializeComponents()
    {
        if (animController == null)
            animController = GetComponent<PlayerAnimationController>();

        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (animController == null)
        {
            Debug.LogError("[PlayerMovement] PlayerAnimationController not found!");
            enabled = false;
            return;
        }

        if (rb == null)
        {
            Debug.LogError("[PlayerMovement] Rigidbody2D not found!");
            enabled = false;
            return;
        }

        // Setup Rigidbody2D
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    // ==========================================
    // INPUT HANDLING
    // ==========================================

    private void HandleInput()
    {
        // Get movement input
        if (useRawInput)
        {
            moveInput = new Vector2(
                Input.GetAxisRaw("Horizontal"),
                Input.GetAxisRaw("Vertical")
            );
        }
        else
        {
            moveInput = new Vector2(
                Input.GetAxis("Horizontal"),
                Input.GetAxis("Vertical")
            );
        }

        // Store original input for debug
        lastInputString = $"({moveInput.x:F2}, {moveInput.y:F2})";

        // Normalize diagonal movement
        if (moveInput.sqrMagnitude > 1f)
        {
            normalizedInput = moveInput.normalized;
        }
        else
        {
            normalizedInput = moveInput;
        }

        // Save last non-zero input for idle direction
        if (normalizedInput.sqrMagnitude > 0.01f)
        {
            lastNonZeroInput = normalizedInput;
            lastDirectionString = GetDirectionName(lastNonZeroInput);
        }

        // Check if running
        isRunning = Input.GetKey(runKey);

        // Debug toggle
        if (Input.GetKeyDown(KeyCode.F1))
        {
            showDebugInfo = !showDebugInfo;
        }
    }

    private void HandleToolInput()
    {
        // Don't allow tool input if action is locked
        if (animController.IsActionLocked)
        {
            return;
        }

        // Hoe (cuốc đất)
        if (Input.GetKeyDown(hoeKey))
        {
            animController.PlayHoeing();
            LogDebug("Used Hoe");
        }
        // Watering (tưới nước)
        else if (Input.GetKeyDown(wateringKey))
        {
            animController.PlayWatering();
            LogDebug("Used Watering Can");
        }
        // Sickle (liềm)
        else if (Input.GetKeyDown(sickleKey))
        {
            animController.PlaySickle();
            LogDebug("Used Sickle");
        }
        // PickUp (nhặt đồ)
        else if (Input.GetKeyDown(pickUpKey))
        {
            animController.PlayPickUp();
            LogDebug("Pick Up");
        }
        // Sleep
        else if (Input.GetKeyDown(sleepKey))
        {
            if (animController.CurrentState == AnimationState.Sleep)
            {
                animController.WakeUp();
                LogDebug("Wake Up");
            }
            else
            {
                animController.PlaySleep();
                LogDebug("Sleep");
            }
        }
    }

    // ==========================================
    // MOVEMENT
    // ==========================================

    private void HandleMovement()
    {
        // Don't move if action is locked or sleeping
        if (animController.IsActionLocked ||
            animController.CurrentState == AnimationState.Sleep)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // Calculate speed
        currentSpeed = moveSpeed;
        if (isRunning && normalizedInput.sqrMagnitude > 0.01f)
        {
            currentSpeed *= runSpeedMultiplier;
        }

        // Apply movement
        Vector2 velocity = normalizedInput * currentSpeed;
        rb.velocity = velocity;
    }

    private void UpdateAnimations()
    {
        // Don't update animations if action is locked
        if (animController.IsActionLocked)
        {
            return;
        }

        // Update animations based on movement
        if (normalizedInput.sqrMagnitude > 0.01f)
        {
            // Moving - pass normalized input to animation controller
            animController.PlayMovement(normalizedInput);

            LogDebug($"Moving: input={lastInputString}, normalized={normalizedInput:F2}, dir={lastDirectionString}");
        }
        else
        {
            // Idle - keep last direction
            animController.PlayIdle();
        }
    }

    // ==========================================
    // HELPER METHODS
    // ==========================================

    private string GetDirectionName(Vector2 dir)
    {
        float absX = Mathf.Abs(dir.x);
        float absY = Mathf.Abs(dir.y);

        if (absY > absX)
        {
            return dir.y > 0 ? "Up" : "Down";
        }
        else
        {
            return dir.x > 0 ? "Right" : "Left";
        }
    }

    // ==========================================
    // PUBLIC METHODS
    // ==========================================

    /// <summary>
    /// Force stop movement và animations
    /// </summary>
    public void ForceStop()
    {
        moveInput = Vector2.zero;
        normalizedInput = Vector2.zero;
        rb.velocity = Vector2.zero;
        animController.ForceStop();
        LogDebug("Force stopped");
    }

    /// <summary>
    /// Set movement enabled/disabled
    /// </summary>
    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            rb.velocity = Vector2.zero;
        }
    }

    /// <summary>
    /// Teleport player to position
    /// </summary>
    public void TeleportTo(Vector2 position)
    {
        transform.position = position;
        rb.velocity = Vector2.zero;
        moveInput = Vector2.zero;
        normalizedInput = Vector2.zero;
    }

    // ==========================================
    // GETTERS
    // ==========================================

    public Vector2 GetMoveInput() => moveInput;
    public Vector2 GetNormalizedInput() => normalizedInput;
    public Vector2 GetLastDirection() => lastNonZeroInput;
    public bool IsMoving() => normalizedInput.sqrMagnitude > 0.01f;
    public bool IsRunning() => isRunning && IsMoving();
    public float GetCurrentSpeed() => currentSpeed;

    // ==========================================
    // DEBUG
    // ==========================================

    private void LogDebug(string message)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[PlayerMovement] {message}");
        }
    }

    private void OnGUI()
    {
        if (!showDebugInfo)
            return;

        // Debug info panel
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.alignment = TextAnchor.UpperLeft;
        boxStyle.fontSize = 12;
        boxStyle.normal.textColor = debugTextColor;
        boxStyle.padding = new RectOffset(10, 10, 10, 10);

        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 12;
        labelStyle.normal.textColor = debugTextColor;

        // Get animator values for debug
        var animator = animController.Animator;
        float animHorizontal = animator.GetFloat("Horizontal");
        float animVertical = animator.GetFloat("Vertical");
        int animState = animator.GetInteger("State");

        string debugText = $@"=== INPUT ===
Raw Input: {lastInputString}
Normalized: ({normalizedInput.x:F2}, {normalizedInput.y:F2})
Last Direction: {lastDirectionString}
Is Moving: {IsMoving()}
Is Running: {IsRunning()}

=== MOVEMENT ===
Speed: {currentSpeed:F1} m/s
Velocity: ({rb.velocity.x:F1}, {rb.velocity.y:F1})
Position: ({transform.position.x:F1}, {transform.position.y:F1})

=== ANIMATION CONTROLLER ===
State: {animController.CurrentState} (Int: {animState})
Direction: {animController.CurrentDirection}
Action Locked: {animController.IsActionLocked}
Sprite FlipX: {animController.IsFacingLeft}

=== ANIMATOR PARAMETERS ===
Horizontal: {animHorizontal:F2}
Vertical: {animVertical:F2}
State: {animState}

=== CONTROLS ===
[WASD/Arrows] - Move
[{runKey}] - Run
[{hoeKey}] - Hoe | [{wateringKey}] - Water | [{sickleKey}] - Sickle
[{pickUpKey}] - Pick Up | [{sleepKey}] - Sleep/Wake
[F1] - Toggle Debug
";

        GUI.Box(new Rect(10, 10, 380, 480), debugText, boxStyle);
    }

#if UNITY_EDITOR
    // Context menu for testing
    [ContextMenu("Test - Force Stop")]
    private void TestForceStop()
    {
        ForceStop();
    }

    [ContextMenu("Test - Teleport to Origin")]
    private void TestTeleportToOrigin()
    {
        TeleportTo(Vector2.zero);
    }

    [ContextMenu("Test - Use Hoe")]
    private void TestUseHoe()
    {
        animController.PlayHoeing();
    }

    [ContextMenu("Test - Toggle Debug")]
    private void TestToggleDebug()
    {
        showDebugInfo = !showDebugInfo;
    }

    [ContextMenu("Debug - Print Current State")]
    private void DebugPrintState()
    {
        Debug.Log("=== CURRENT STATE ===");
        Debug.Log($"Move Input: {moveInput}");
        Debug.Log($"Normalized Input: {normalizedInput}");
        Debug.Log($"Last Direction: {lastNonZeroInput}");
        Debug.Log($"Current Speed: {currentSpeed}");
        Debug.Log($"Anim State: {animController.CurrentState}");
        Debug.Log($"Anim Direction: {animController.CurrentDirection}");

        var animator = animController.Animator;
        Debug.Log($"Animator Horizontal: {animator.GetFloat("Horizontal")}");
        Debug.Log($"Animator Vertical: {animator.GetFloat("Vertical")}");
        Debug.Log($"Animator State: {animator.GetInteger("State")}");
    }

    // Draw movement direction in Scene view
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying || !showDebugGizmos)
            return;

        Vector3 pos = transform.position;

        // Draw current movement input (cyan/red line)
        if (normalizedInput.sqrMagnitude > 0.01f)
        {
            Gizmos.color = isRunning ? Color.red : Color.cyan;
            Gizmos.DrawLine(pos, pos + (Vector3)normalizedInput * 1.5f);
            Gizmos.DrawSphere(pos + (Vector3)normalizedInput * 1.5f, 0.1f);
        }

        // Draw last direction (yellow line)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos, pos + (Vector3)lastNonZeroInput * 1f);

        // Draw direction text
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(
            pos + Vector3.up * 2f,
            $"{lastDirectionString}\nInput: {lastInputString}\nAnim: {animController.CurrentDirection}"
        );

        // Draw coordinate system
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + Vector3.up * 0.5f); // Y axis (Up)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + Vector3.right * 0.5f); // X axis (Right)
    }
#endif

}
