using System.Collections;
using System.Collections.Generic;
using TinyFarm.Animation;
using TinyFarm.Items;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private PlayerAnimationController animController;
    private Vector2 moveInput = Vector2.zero;

    [Header("Test Settings")]
    [Tooltip("Tốc độ di chuyển giả lập (chỉ để đổi hướng animation).")]
    [SerializeField] private float moveSpeed = 2f;

    [Tooltip("Tool hiện tại đang được chọn.")]
    [SerializeField] private ToolType currentTool = ToolType.Hoe;

    [Tooltip("Bật hiển thị log trạng thái.")]
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        animController = GetComponent<PlayerAnimationController>();
    }

    private void Update()
    {
        HandleMovementInput();
        HandleToolInput();
    }

    private void HandleMovementInput()
    {
        // Lấy input WASD
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (moveInput.sqrMagnitude > 0.01f)
        {
            animController.PlayWalkingAnimation(moveInput.normalized);
        }
        else
        {
            animController.PlayIdleAnimation();
        }
    }

    private void HandleToolInput()
    {
        // Đổi tool bằng phím số
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetTool(ToolType.Hoe);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetTool(ToolType.Watering);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetTool(ToolType.Sickle);
        if (Input.GetKeyDown(KeyCode.Alpha4)) SetTool(ToolType.PickUp);

        // Thực hiện hành động bằng SPACE
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryUseTool();
        }
    }

    private void TryUseTool()
    {
        if (animController.CanPerformAction())
        {
            animController.PlayToolAnimation(currentTool);
            if (showDebugLogs)
                Debug.Log($"▶️ Dùng tool: {currentTool}");
        }
        else if (showDebugLogs)
        {
            Debug.Log($"⏳ Đang thực hiện hành động khác...");
        }
    }

    private void SetTool(ToolType tool)
    {
        currentTool = tool;
        animController.SetCurrentTool(tool);
        if (showDebugLogs)
            Debug.Log($"🛠 Đổi tool sang: {tool}");
    }

    private void OnDrawGizmos()
    {
        if (moveInput.sqrMagnitude > 0.01f)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + (Vector3)moveInput.normalized * 0.5f);
        }
    }
}
