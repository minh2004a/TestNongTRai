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
    }

    private void Update()
    {
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
            enabled = false;
            return;
        }

        if (rb == null)
        {
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
        rb.velocity = normalizedInput * currentSpeed;
    }

    private void UpdateAnimations()
    {
        if (animController.IsActionLocked) return;

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
    // GETTERS
    // ==========================================

    public Vector2 GetMoveInput() => moveInput;
    public Vector2 GetNormalizedInput() => normalizedInput;
    public Vector2 GetLastDirection() => lastNonZeroInput;
    public bool IsMoving() => normalizedInput.sqrMagnitude > 0.01f;
    public float GetCurrentSpeed() => currentSpeed;
}
