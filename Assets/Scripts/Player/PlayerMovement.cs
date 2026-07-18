using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 12f;
    public float dashSpeed = 18f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.6f;

    [Header("Ground / Wall Check")]
    public Transform groundCheck;      // assign your GroundCheck object here
    public float groundCheckRadius = 0.15f;
    public LayerMask groundLayer;

    public Transform wallCheck;        // empty object placed at the character's side
    public bool isWallJumping;
    public float wallCheckRadius = 0.2f;
    public LayerMask wallLayer;
    public float wallSlideSpeed = 2f;
    public Vector2 wallJumpForce = new Vector2(10f, 12f); // horizontal and vertical force applied when jumping off a wall
    public float WallJumpDuration = 0.5f;
    public float WallJumpCounter;
    public float WallJumpDirection; // -1 for left, 1 for right
    public float WallJumpTime = 0.2f; // Time window to allow wall jump after leaving the wall

    [Header("References")]
    public Animator animator;
    public Rigidbody2D rb;
    public SpriteRenderer spriteRenderer;

    // internal state
    private float horizontalInput;
    private bool isGrounded;
    private bool isWalling;
    private bool hasJumped;       // true once the first (ground) jump has been used
    private bool hasDoubleJumped; // true once the double jump has been used
    private bool isDashing;
    private bool dashOnCooldown;
    private bool facingRight = true;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // ---- Input ----
        horizontalInput = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right arrows

        // ---- Ground / Wall detection ----
        isGrounded = groundCheck != null &&
                     Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        bool touchingWall = wallCheck != null &&
                             Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);

        // Only "wall slide" if pressing into the wall, airborne, and falling
        isWalling = touchingWall && !isGrounded && horizontalInput != 0 && rb.linearVelocity.y < 0;

        if (isGrounded)
        {
            // Landed (or standing) on the ground - fully reset the jump chain
            hasJumped = false;
            hasDoubleJumped = false;
        }

        // TEMP DEBUG - remove once jump is confirmed working
        Debug.Log($"isGrounded={isGrounded}  hasJumped={hasJumped}  hasDoubleJumped={hasDoubleJumped}  animator={(animator != null ? "OK" : "NULL")}");

        // ---- Flip ----
        HandleFlip();

        // ---- Jump (Spacebar) ----
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded && !hasJumped)
            {
                // First jump - only usable from the ground
                Jump();
                hasJumped = true;
                animator.SetTrigger("Jump");
                Debug.Log("JUMP fired - trigger set, velocity=" + rb.linearVelocity);
            }
            else if (hasJumped && !hasDoubleJumped)
            {
                // Second jump - only usable once, after the first jump, while airborne
                Jump();
                hasDoubleJumped = true;
                animator.SetTrigger("DoubleJump");
                Debug.Log("DOUBLEJUMP fired - trigger set, velocity=" + rb.linearVelocity);
            }
            else
            {
                Debug.Log("Space pressed but BLOCKED - isGrounded=" + isGrounded + " hasJumped=" + hasJumped + " hasDoubleJumped=" + hasDoubleJumped);
            }
        }

        // ---- Dash (J) ----
        if (Input.GetKeyDown(KeyCode.J) && !isDashing && !dashOnCooldown)
        {
            StartCoroutine(DoDash());
        }

        // ---- Update Animator parameters every frame ----
        UpdateAnimatorParams();
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            // Normal horizontal movement; don't override velocity while wall-sliding grip if you want a slower slide
            if (isWalling)
            {
                // slow controlled slide down the wall
                rb.linearVelocity = new Vector2(0f, Mathf.Max(rb.linearVelocity.y, -2f));
            }
            else
            {
                rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
            }
        }
    }

    void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    System.Collections.IEnumerator DoDash()
    {
        isDashing = true;
        dashOnCooldown = true;
        animator.SetBool("Dash", true);

        float dashDir = facingRight ? 1f : -1f;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = new Vector2(dashDir * dashSpeed, 0f);

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
        animator.SetBool("Dash", false);

        yield return new WaitForSeconds(dashCooldown);
        dashOnCooldown = false;
    }

    void WallJump()
    {
        if (isWalling)
        {
            // Determine the direction to jump away from the wall
            WallJumpDirection = facingRight ? -1f : 1f;
            // Apply the wall jump force
            rb.linearVelocity = new Vector2(WallJumpDirection * wallJumpForce.x, wallJumpForce.y);
            // Temporarily disable horizontal input for a short duration to prevent immediate re-grabbing the wall
            isWallJumping = false;
            WallJumpCounter = WallJumpDuration;
        }
        else
        {
            WallJumpCounter -= Time.deltaTime;
        }

        // Use GetButtonDown for input mapped to "Jump" (string); GetMouseButtonDown requires an int
        if (Input.GetButtonDown("Jump") && WallJumpCounter > 0f) {
            rb.linearVelocity = new Vector2(WallJumpDirection * wallJumpForce.x, wallJumpForce.y);
            WallJumpCounter = 0f;
            if (facingRight && WallJumpDirection < 0f)
            {
                Flip();
            } else if (!facingRight && WallJumpDirection > 0f){
                Flip();
            } else
            {
                // Do nothing, already facing the correct direction
            }
        }
    }

    void StopWallJumping()
    {

    }

    void HandleFlip()
    {
        if (horizontalInput > 0 && !facingRight)
        {
            Flip();
        }
        else if (horizontalInput < 0 && facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;

        // Alternative if you prefer not to flip the whole transform (e.g. child colliders shouldn't flip):
        // spriteRenderer.flipX = !spriteRenderer.flipX;
    }

    void UpdateAnimatorParams()
    {
        // magnitude drives Idle <-> Run
        animator.SetFloat("magnitude", Mathf.Abs(horizontalInput));

        // yVelocity drives Fall / Jump / DoubleJump / WallSlide transitions
        animator.SetFloat("yVelocity", rb.linearVelocity.y);

        // bools
        animator.SetBool("isGround", isGrounded);
        animator.SetBool("Walling", isWalling);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
    }
}