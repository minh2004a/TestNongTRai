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
    [SerializeField] private Animator animator;
 
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;

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

    /// Set move input từ PlayerInputHandler
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
            return;

        bool hasMove = normalizedInput.sqrMagnitude > 0.01f;

        if (animController.IsCarrying)
        {
            // Nếu có input → chạy pickup run
            if (hasMove)
            {
                animController.PlayPickUpRun();
                animController.UpdateDirection(normalizedInput);
                animController.UpdateDirectionParameters(normalizedInput);
                animController.UpdateSpriteFlip(normalizedInput);
            }
            else
            {
                // Không di chuyển → pickup idle
                animController.PlayPickUpIdle();
                animController.UpdateDirectionParameters(lastNonZeroInput);
                animController.UpdateSpriteFlip(lastNonZeroInput);
            }
            return;
        }

        // Không cầm đồ → animation thường
        if (hasMove)
        {
            animController.PlayMovement(normalizedInput);
        }
        else
        {
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

    /// Force stop movement và animations
    public void ForceStop()
    {
        moveInput = Vector2.zero;
        normalizedInput = Vector2.zero;
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
        
    }
}
