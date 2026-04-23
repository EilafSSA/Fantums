using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("=== Movement ===")]
    [SerializeField] private float moveSpeed = 7f;
    [SerializeField] private float jumpForce = 14f;

    [Header("=== Ground Check ===")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.3f;
    [SerializeField] private LayerMask groundLayer;

    [Header("=== Grab / Cling ===")]
    [SerializeField] private Transform grabCheck;
    [SerializeField] private float grabCheckRadius = 0.4f;
    [SerializeField] private LayerMask clingLayer;
    [SerializeField] private float clingJumpForceX = 6f;
    [SerializeField] private float clingJumpForceY = 12f;

    [Header("=== Attack ===")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange = 0.6f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private float attackInputCooldown = 0.08f; // tiny just to stop spamclick abuse
    [SerializeField] private float comboResetTime = 0.6f;       // if u dont click again within this window combo resets
    [SerializeField] private int maxComboStep = 3;              // Attack_1 -> Attack_2 -> Attack_3
    [SerializeField] private LayerMask enemyLayer;

    [Header("=== Dash ===")]
    [SerializeField] private float dashSpeed = 18f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.8f;

    private Animator anim; //addedbyEilaf
    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool isClinging;
    private bool facingRight = true;
    private float attackTimer;

    // combo tracking
    private int comboStep = 0;
    private float comboResetTimer;

    // dashhh
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float dashDirection;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); //addedbyEilaf - moved here so it only runs once instead of every frame
    }

    private void Update()
    {
        // don't process input while dashing (we can remove this later if we want anim-cancel or diractiloanl dash idk but good to know)
        if (isDashing) return;

        moveInput = Input.GetAxisRaw("Horizontal");

        isGrounded = Physics2D.OverlapBox(
            groundCheck.position,
            new Vector2(0.4f, groundCheckRadius),
            0f,
            groundLayer
        );

        if (isGrounded)
        {
            anim.SetBool("IsGrounded", true);
            anim.SetBool("IsClinging", false);
        }
        else { anim.SetBool("IsGrounded", false); anim.SetBool("IsClinging", false); } //addedbyEilaf

        bool clingContact = Physics2D.OverlapCircle(grabCheck.position, grabCheckRadius, clingLayer);

        if (clingContact && !isGrounded && Input.GetMouseButtonDown(1) && !isClinging)
        {
            isClinging = true;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        if (isClinging && Input.GetMouseButtonUp(1)) ReleaseCling();
        if (isClinging && !clingContact) ReleaseCling();

        if (Input.GetButtonDown("Jump"))
        {
            if (isClinging)
            {
                float awayDir = facingRight ? 1f : -1f;
                rb.gravityScale = 3f;
                isClinging = false;
                rb.linearVelocity = Vector2.zero;
                rb.AddForce(new Vector2(awayDir * clingJumpForceX, clingJumpForceY), ForceMode2D.Impulse);
            }
            else if (isGrounded)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                anim.SetTrigger("Jump"); //addedbyEilaf
            }
        }

        if (isClinging) { anim.SetBool("IsClinging", true); } //addedbyEilaf
        else { anim.SetBool("IsClinging", false); } //addedbyEilaf

        // dash input
        dashCooldownTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.LeftShift) && dashCooldownTimer <= 0f && !isClinging)
        {
            dashDirection = moveInput != 0f ? Mathf.Sign(moveInput) : (facingRight ? 1f : -1f);
            StartCoroutine(DashRoutine());
        }

        // if the player stops clicking midcombo, this quietly resets the chain back to 0
        if (comboStep > 0)
        {
            comboResetTimer -= Time.deltaTime;
            if (comboResetTimer <= 0f)
            {
                comboStep = 0;
                anim.SetInteger("ComboStep", 0);
            }
        }

        // === ATTACK INPUT ===
        attackTimer -= Time.deltaTime;
        if (Input.GetMouseButtonDown(0) && attackTimer <= 0f)
        {
            Attack();
            attackTimer = attackInputCooldown;
        }

        if (!isClinging)
        {
            if (moveInput > 0 && !facingRight) Flip();
            else if (moveInput < 0 && facingRight) Flip();
        }
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f); // zero Y so no gravity drift during dash
            return;
        }

        if (isClinging)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            anim.SetBool("IsClinging", true); //addedbyEilaf
            anim.SetBool("IsRunning", false); //addedbyEilaf
        }
        else
        {
            rb.gravityScale = 3f;
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
            anim.SetBool("IsRunning", true); //addedbyEilaf
            anim.SetBool("IsClinging", false); //addedbyEilaf
        }
    }

    private System.Collections.IEnumerator DashRoutine()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;

        rb.gravityScale = 0f;           // kill gravity so the dash stays level
        anim.SetTrigger("Dash");        // hook up a Dash trigger in Animator if you wanna make one!! :3

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
        rb.gravityScale = 3f;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.3f, rb.linearVelocity.y); // bleed off dash speed smoothly
    }

    private void ReleaseCling()
    {
        isClinging = false;
        rb.gravityScale = 3f;
        anim.SetBool("IsClinging", false); //addedbyEilaf
    }

    private void Attack()
    {
        // advance combo step bf4 firing the trigger so the animator sees the right int
        if (comboStep < maxComboStep)
            comboStep++;
        else
            comboStep = 1; // loop back to the start of the combo once we finish all 3

        anim.SetInteger("ComboStep", comboStep);
        anim.SetTrigger("Attack"); //addedbyEilaf

        comboResetTimer = comboResetTime;

        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemyHP = hit.GetComponent<EnemyHealth>();
            if (enemyHP == null)
                enemyHP = hit.GetComponentInParent<EnemyHealth>();

            if (enemyHP != null)
                enemyHP.TakeDamage(attackDamage);
        }
    }


    public void ResetCombo()
    {
        comboStep = 0;
        anim.SetInteger("ComboStep", 0);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1;
        transform.localScale = s;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, new Vector3(0.4f, groundCheckRadius * 2f, 0f));
        }
        if (grabCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(grabCheck.position, grabCheckRadius);
        }
        if (attackPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }
}