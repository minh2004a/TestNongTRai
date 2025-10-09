using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : GameMonoBehaviour
{
    [SerializeField] public float speed = 100f;
    protected Vector2 input;

    [SerializeField] protected Rigidbody2D rb;
    public Rigidbody2D Rigidbody2D => rb;

    [SerializeField] private Animator animator;
    public Animator Animator => animator;

    [SerializeField] private SpriteRenderer spriteRenderer;
    public SpriteRenderer SpriteRenderer => spriteRenderer;

    private Vector2 lastMoveDir = Vector2.down; // hướng mặc định khi bắt đầu game
    protected override void Awake()
    {
        base.Awake();
        this.LoadComponents();
    }

    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadRigidbody();
        this.LoadAnimator();
        this.LoadSpriteRenderer();
    }

    protected virtual void LoadRigidbody()
    {
        if (rb != null) return;
        this.rb = GetComponentInParent<Rigidbody2D>();
        Debug.Log(transform.name + ": LoadRigidbody", gameObject);
    }

    protected virtual void LoadAnimator()
    {
        if (animator != null) return;
        this.animator = transform.parent.GetComponentInChildren<Animator>();
        Debug.Log(transform.name + ": LoadAnimator", gameObject);
    }

    protected virtual void LoadSpriteRenderer()
    {
        if (spriteRenderer != null) return;
        this.spriteRenderer = transform.parent.GetComponentInChildren<SpriteRenderer>();
        Debug.Log(transform.name + ": LoadSpriteRenderer", gameObject);
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
