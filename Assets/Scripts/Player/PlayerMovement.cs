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
    [SerializeField] private KeyCode hoeKey = KeyCode.J;
    [SerializeField] private KeyCode wateringKey = KeyCode.K;
    [SerializeField] private KeyCode sickleKey = KeyCode.L;
    [SerializeField] private KeyCode pickUpKey = KeyCode.E;
    [SerializeField] private KeyCode sleepKey = KeyCode.R;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private Color debugTextColor = Color.white;

    // Runtime
    private Vector2 moveInput;
    private Vector2 lastNonZeroInput;
    private bool isRunning;
    private float currentSpeed;


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
    }

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

        // Normalize diagonal movement
        if (moveInput.sqrMagnitude > 1f)
        {
            moveInput.Normalize();
        }

        // Save last non-zero input for idle direction
        if (moveInput.sqrMagnitude > 0.01f)
        {
            lastNonZeroInput = moveInput;
        }

        // Check if running
        isRunning = Input.GetKey(runKey);
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
        if (isRunning && moveInput.sqrMagnitude > 0.01f)
        {
            currentSpeed *= runSpeedMultiplier;
        }

        // Apply movement
        Vector2 velocity = moveInput * currentSpeed;
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
        if (moveInput.sqrMagnitude > 0.01f)
        {
            // Moving
            animController.PlayMovement(moveInput);
        }
        else
        {
            // Idle - keep last direction
            animController.PlayIdle();
        }
    }

    /// Force stop movement và animations
    public void ForceStop()
    {
        moveInput = Vector2.zero;
        rb.velocity = Vector2.zero;
        animController.ForceStop();
        LogDebug("Force stopped");
    }

    /// Set movement enabled/disabled
    public void SetMovementEnabled(bool enabled)
    {
        this.enabled = enabled;
        if (!enabled)
        {
            rb.velocity = Vector2.zero;
        }
    }

    /// Teleport player to position
    public void TeleportTo(Vector2 position)
    {
        transform.position = position;
        rb.velocity = Vector2.zero;
        moveInput = Vector2.zero;
    }

    public Vector2 GetMoveInput() => moveInput;
    public Vector2 GetLastDirection() => lastNonZeroInput;
    public bool IsMoving() => moveInput.sqrMagnitude > 0.01f;
    public bool IsRunning() => isRunning && IsMoving();
    public float GetCurrentSpeed() => currentSpeed;

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
        GUIStyle style = new GUIStyle(GUI.skin.box);
        style.alignment = TextAnchor.UpperLeft;
        style.fontSize = 14;
        style.normal.textColor = debugTextColor;

        string debugText = $@"
=== PLAYER MOVEMENT DEBUG ===
Movement Input: {moveInput.ToString("F2")}
Last Direction: {lastNonZeroInput.ToString("F2")}
Speed: {currentSpeed:F1} m/s
Is Moving: {IsMoving()}
Is Running: {IsRunning()}
Velocity: {rb.velocity.ToString("F2")}

=== ANIMATION STATE ===
Current State: {animController.CurrentState}
Direction: {animController.CurrentDirection}
Action Locked: {animController.IsActionLocked}
Is Moving (Anim): {animController.IsMoving}

=== CONTROLS ===
[WASD/Arrows] - Move
[{runKey}] - Run
[{hoeKey}] - Hoe
[{wateringKey}] - Watering
[{sickleKey}] - Sickle
[{pickUpKey}] - Pick Up
[{sleepKey}] - Sleep/Wake Up
[F1] - Toggle Debug
";

        GUI.Box(new Rect(10, 10, 350, 400), debugText, style);
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

    // Draw movement direction in Scene view
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
            return;

        // Draw movement input
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Gizmos.color = isRunning ? Color.red : Color.green;
            Gizmos.DrawLine(transform.position,
                (Vector2)transform.position + moveInput * 2f);
        }

        // Draw last direction
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position,
            (Vector2)transform.position + lastNonZeroInput);
    }
#endif
}
