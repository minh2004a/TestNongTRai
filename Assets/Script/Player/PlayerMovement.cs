using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] public float speed = 100f;
    protected Vector2 input;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private Vector2 lastMoveDir = Vector2.down; // hướng mặc định khi bắt đầu game
    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponentInParent<Rigidbody2D>();
        }
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        this.InputMove();
        this.HandleInput();
    }

    private void FixedUpdate()
    {
        this.Move();
    }

    protected virtual void InputMove()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        input = new Vector2(x, y).normalized;
    }

    protected virtual void Move()
    {
        rb.velocity = input * speed * Time.fixedDeltaTime;
    }

    private void HandleInput()
    {
        bool isMoving = input.magnitude > 0.1f;
        animator.SetBool("isMoving", isMoving);

        // Cập nhật hướng cuối cùng nếu đang di chuyển
        if (isMoving)
        {
            lastMoveDir = input;
        }

        // Gửi hướng cuối cùng sang Animator (để Idle hiển thị đúng hướng)
        animator.SetFloat("Horizontal", lastMoveDir.x);
        animator.SetFloat("Vertical", lastMoveDir.y);

        // Lật sprite nếu đi trái
        if (lastMoveDir.x < -0.1f) spriteRenderer.flipX = true;
        else if (lastMoveDir.x > 0.1f) spriteRenderer.flipX = false;
    }
}
