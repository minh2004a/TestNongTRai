using System.Collections;
using System.Collections.Generic;
using TinyFarm.Animation;
using TinyFarm.Items;
using UnityEngine;
using static Unity.Collections.Unicode;

[RequireComponent(typeof(PlayerAnimationController))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAnimationController animController;
    [SerializeField] private Rigidbody2D rb;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showDebugGizmos = true;
    [SerializeField] private Color debugTextColor = Color.white;

    // Runtime
    private Vector2 moveInput;
    private Vector2 normalizedInput;
    private Vector2 lastNonZeroInput;
    private float currentSpeed;

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
        // Input handled by PlayerInputHandler now
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
    // PUBLIC API - Called by PlayerInputHandler
    // ==========================================

    /// <summary>
    /// Set move input từ PlayerInputHandler
    /// </summary>
    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;

        // Normalize diagonal movement
        if (moveInput.sqrMagnitude > 1f)
        {
            normalizedInput = moveInput.normalized;
        }
        else
        {
            normalizedInput = moveInput;
        }

        // Save last non-zero input
        if (normalizedInput.sqrMagnitude > 0.01f)
        {
            lastNonZeroInput = normalizedInput;
        }
    }

    // ==========================================
    // MOVEMENT (Now controlled by PlayerInputHandler)
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

        // Calculate speed (no sprint key, constant speed)
        currentSpeed = moveSpeed;

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

        // Get animator values for debug
        var animator = animController.Animator;
        float animHorizontal = animator.GetFloat("Horizontal");
        float animVertical = animator.GetFloat("Vertical");
        int animState = animator.GetInteger("State");

        string debugText = $@"=== PLAYER MOVEMENT DEBUG ===
Move Input: ({moveInput.x:F2}, {moveInput.y:F2})
Normalized: ({normalizedInput.x:F2}, {normalizedInput.y:F2})
Last Direction: {GetDirectionName(lastNonZeroInput)}
Is Moving: {IsMoving()}

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

=== NOTE ===
Input handled by PlayerInputHandler
Use PlayerInputHandler debug for input info
";

        GUI.Box(new Rect(10, 10, 380, 450), debugText, boxStyle);
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

    [ContextMenu("Test - Toggle Debug")]
    private void TestToggleDebug()
    {
        showDebugInfo = !showDebugInfo;
    }

    [ContextMenu("Debug - Print Current State")]
    private void DebugPrintState()
    {
        Debug.Log("=== MOVEMENT STATE ===");
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

        // Draw current movement input (cyan line)
        if (normalizedInput.sqrMagnitude > 0.01f)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(pos, pos + (Vector3)normalizedInput * 1.5f);
            Gizmos.DrawSphere(pos + (Vector3)normalizedInput * 1.5f, 0.1f);
        }

        // Draw last direction (yellow line)
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(pos, pos + (Vector3)lastNonZeroInput * 1f);

        // Draw direction text
        if (animController != null)
        {
            UnityEditor.Handles.color = Color.white;
            UnityEditor.Handles.Label(
                pos + Vector3.up * 2f,
                $"{GetDirectionName(lastNonZeroInput)}\nInput: ({moveInput.x:F1}, {moveInput.y:F1})\nAnim: {animController.CurrentDirection}"
            );
        }

        // Draw coordinate system
        Gizmos.color = Color.green;
        Gizmos.DrawLine(pos, pos + Vector3.up * 0.5f); // Y axis (Up)
        Gizmos.color = Color.red;
        Gizmos.DrawLine(pos, pos + Vector3.right * 0.5f); // X axis (Right)
    }
#endif
}
