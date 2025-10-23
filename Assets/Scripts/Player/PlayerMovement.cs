using System.Collections;
using System.Collections.Generic;
using TinyFarm.Animation;
using TinyFarm.Items;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerAnimationController animController;
    private Rigidbody2D rb;

    private Vector2 moveInput;
    private ToolType currentTool = ToolType.Hoe;
    private Direction currentDir = Direction.Down;

    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private bool faceMovementDirection = true;

    [Header("Debug")]
    [SerializeField] private bool showLogs = true;

    private void Awake()
    {
        animController = GetComponent<PlayerAnimationController>();
        rb = GetComponent<Rigidbody2D>();

        // Cấu hình Rigidbody2D (nếu chưa)
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    private void Update()
    {
        HandleInput();
        HandleToolSwitch();
        HandleDirectionSwitch();
        HandleToolAction();
    }

    private void FixedUpdate()
    {
        MoveCharacter();
    }

    // ============================================
    // INPUT HANDLING
    // ============================================

    private void HandleInput()
    {
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveInput = moveInput.normalized;

        if (moveInput.sqrMagnitude > 0.01f)
        {
            animController.PlayWalkingAnimation(moveInput);

            if (faceMovementDirection)
                UpdateFacingDirection(moveInput);
        }
        else
        {
            animController.PlayIdleAnimation();
        }
    }

    private void HandleToolSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetTool(ToolType.Hoe);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetTool(ToolType.Watering);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetTool(ToolType.Sickle);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetTool(ToolType.PickUp);
    }

    private void HandleDirectionSwitch()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentDir = (Direction)(((int)currentDir + 1) % 3);
            if (showLogs) Debug.Log($"↩️ Đổi hướng: {currentDir}");
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentDir = (Direction)(((int)currentDir + 2) % 3);
            if (showLogs) Debug.Log($"↪️ Đổi hướng: {currentDir}");
        }
    }

    private void HandleToolAction()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (animController.CanPerformAction())
            {
                animController.PlayToolAnimation(currentTool, currentDir);
                if (showLogs)
                    Debug.Log($"🎬 Play Tool Animation: {currentTool} | {currentDir}");
            }
            else if (showLogs)
            {
                Debug.Log($"⏳ Đang thực hiện hành động khác...");
            }
        }
    }

    // ============================================
    // MOVEMENT
    // ============================================

    private void MoveCharacter()
    {
        rb.velocity = moveInput * moveSpeed;
    }

    private void UpdateFacingDirection(Vector2 dir)
    {
        // Cập nhật hướng nhìn và flip sprite
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            currentDir = Direction.Side;
        }
        else
        {
            currentDir = dir.y > 0 ? Direction.Up : Direction.Down;
        }
    }

    private void SetTool(ToolType tool)
    {
        currentTool = tool;
        animController.SetCurrentTool(tool);
        if (showLogs)
            Debug.Log($"🛠 Đổi tool sang: {tool}");
    }

    // ============================================
    // DEBUG GIZMOS
    // ============================================

    private void OnDrawGizmos()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)moveInput.normalized * 0.5f);
        }
    }
}
