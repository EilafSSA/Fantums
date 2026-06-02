using UnityEngine;
using UnityEngine.InputSystem;

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

    [Header("=== Gamepad ===")]
    [SerializeField] private float gamepadStickDeadzone = 0.25f;
    [SerializeField] private float gamepadTriggerThreshold = 0.5f;

    [Header("=== Input (Input System) ===")]
    [Tooltip("Drag actions from InputSystem_Actions > Player here.")]
    [SerializeField] private InputActionReference moveAction;
    [SerializeField] private InputActionReference jumpAction;
    [SerializeField] private InputActionReference attackAction;
    [SerializeField] private InputActionReference sprintAction;
    [SerializeField] private InputActionReference clingAction;

    private Animator anim; //addedbyEilaf
    private Rigidbody2D rb;
    private float moveInput;
    private bool isGrounded;
    private bool isClinging;
    private bool facingRight = true;
    private float attackTimer;

    private bool prevAttackHeld;
    private bool prevDashHeld;
    private bool prevClingHeld;
    private bool prevJumpHeld;

    // combo tracking
    private int comboStep = 0;
    private float comboResetTimer;

    // dashhh
    private bool isDashing;
    private float dashTimer;
    private float dashCooldownTimer;
    private float dashDirection;

    private Transform currentClingTarget;

    private bool isEndingCutscene = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); //addedbyEilaf - moved here so it only runs once instead of every frame
    }

    private void OnEnable()
    {
        EnableAction(moveAction);
        EnableAction(jumpAction);
        EnableAction(attackAction);
        EnableAction(sprintAction);
        EnableAction(clingAction);
    }

    private static void EnableAction(InputActionReference reference)
    {
        if (reference != null && reference.action != null)
            reference.action.Enable();
    }

    // pull a live action: prefer the assigned reference, fall back to InputManager so saved rebinds apply
    private InputAction Resolve(InputActionReference reference, System.Func<InputManager, InputAction> fromManager)
    {
        if (reference != null && reference.action != null)
            return reference.action;
        if (InputManager.Instance != null)
            return fromManager(InputManager.Instance);
        return null;
    }

    private float ReadMoveAxis()
    {
        InputAction a = Resolve(moveAction, m => m.Move);
        if (a == null) return 0f;
        float x = a.ReadValue<Vector2>().x;
        if (Mathf.Abs(x) < gamepadStickDeadzone) x = 0f;
        return Mathf.Clamp(x, -1f, 1f);
    }

    private bool JumpHeld()
    {
        InputAction a = Resolve(jumpAction, m => m.Jump);
        return a != null && a.IsPressed();
    }

    private bool AttackHeld()
    {
        InputAction a = Resolve(attackAction, m => m.Attack);
        return a != null && a.IsPressed();
    }

    private bool DashHeld()
    {
        InputAction a = Resolve(sprintAction, m => m.Sprint);
        return a != null && a.IsPressed();
    }

    private bool ClingHeld()
    {
        InputAction a = Resolve(clingAction, m => m.Cling);
        return a != null && a.IsPressed();
    }

    private void Update()
    {
        if (isEndingCutscene) return;

        // don't process input while dashing (we can remove this later if we want anim-cancel or diractiloanl dash idk but good to know)
        if (isDashing) return;

        moveInput = ReadMoveAxis();

        bool jumpHeld = JumpHeld();
        bool jumpDown = jumpHeld && !prevJumpHeld;
        prevJumpHeld = jumpHeld;

        bool attackHeld = AttackHeld();
        bool attackDown = attackHeld && !prevAttackHeld;
        prevAttackHeld = attackHeld;

        bool dashHeld = DashHeld();
        bool dashDown = dashHeld && !prevDashHeld;
        prevDashHeld = dashHeld;

        bool clingHeld = ClingHeld();
        bool clingDown = clingHeld && !prevClingHeld;
        bool clingUp = !clingHeld && prevClingHeld;
        prevClingHeld = clingHeld;

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

        Collider2D clingCollider = Physics2D.OverlapCircle(grabCheck.position, grabCheckRadius, clingLayer);
        bool clingContact = clingCollider != null;

        if (clingContact && !isGrounded && clingDown && !isClinging)
        {
            isClinging = true;
            currentClingTarget = clingCollider.transform;
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
        }

        if (isClinging && clingUp) ReleaseCling();
        if (isClinging && !clingContact) ReleaseCling();

        if (jumpDown)
        {
            if (isClinging)
            {
                float awayDir = facingRight ? 1f : -1f;
                rb.gravityScale = 3f;
                isClinging = false;
                currentClingTarget = null;
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
        if (dashDown && dashCooldownTimer <= 0f && !isClinging)
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
        if (attackDown && attackTimer <= 0f)
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
        if (isEndingCutscene) return;

        if (isDashing)
        {
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            return;
        }

        if (isClinging)
        {
            if (currentClingTarget != null)
            {
                transform.position = currentClingTarget.position;
            }
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            anim.SetBool("IsClinging", true);
            anim.SetBool("IsRunning", false);
        }
        else
        {
            rb.gravityScale = 3f;
            rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
            anim.SetBool("IsRunning", true);
            anim.SetBool("IsClinging", false);
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
        currentClingTarget = null;
        rb.gravityScale = 3f;
        anim.SetBool("IsClinging", false);
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
            {
                enemyHP.TakeDamage(attackDamage);
                continue;
            }

            FinalBossArm bossArm = hit.GetComponent<FinalBossArm>();
            if (bossArm == null) bossArm = hit.GetComponentInParent<FinalBossArm>();

            if (bossArm != null)
            {
                bossArm.TakeDamage(attackDamage);
                continue;
            }

            ShadowBossHitbox bossHitbox = hit.GetComponent<ShadowBossHitbox>();
            if (bossHitbox == null)
                bossHitbox = hit.GetComponentInParent<ShadowBossHitbox>();

            if (bossHitbox != null)
                bossHitbox.TakeDamage(attackDamage);
        }
        
        Collider2D[] allHits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);
        foreach (Collider2D hit in allHits)
        {
            FinalBossArm bossArm = hit.GetComponent<FinalBossArm>();
            if (bossArm != null)
            {
                bossArm.TakeDamage(attackDamage);
                break;
            }

            ShadowBossHitbox bossHitbox = hit.GetComponent<ShadowBossHitbox>();
            if (bossHitbox != null)
            {
                bossHitbox.TakeDamage(attackDamage);
                break;
            }
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

    public void StartEndingCutscene(System.Collections.Generic.List<EndingWaypoint> waypoints)
    {
        if (isEndingCutscene) return;
        StartCoroutine(EndingRoutine(waypoints));
    }

    private System.Collections.IEnumerator EndingRoutine(System.Collections.Generic.List<EndingWaypoint> waypoints)
    {
        isEndingCutscene = true;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        if (anim != null)
        {
            anim.SetBool("IsGrounded", true);
            anim.SetBool("IsClinging", false);
            anim.SetInteger("ComboStep", 0);
        }

        for (int i = 0; i < waypoints.Count; i++)
        {
            EndingWaypoint wp = waypoints[i];
            Vector3 startPos = transform.position;
            Vector3 targetPos = new Vector3(wp.position.x, wp.position.y, transform.position.z);

            float direction = targetPos.x - startPos.x;
            if (Mathf.Abs(direction) > 0.01f)
            {
                if (direction > 0 && !facingRight) Flip();
                else if (direction < 0 && facingRight) Flip();
            }

            if (anim != null)
            {
                anim.SetBool("IsRunning", wp.playRunningAnim);
            }

            float distance = Vector3.Distance(startPos, targetPos);
            if (distance > 0.01f && wp.speed > 0f)
            {
                float duration = distance / wp.speed;
                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    transform.position = Vector3.Lerp(startPos, targetPos, t);
                    yield return null;
                }
            }

            transform.position = targetPos;

            if (anim != null)
            {
                anim.SetBool("IsRunning", false);
            }

            if (wp.pauseDuration > 0f)
            {
                yield return new WaitForSeconds(wp.pauseDuration);
            }
        }
    }
}

[System.Serializable]
public class EndingWaypoint
{
    public string waypointName;
    public Vector2 position;
    public float speed = 5f;
    public float pauseDuration = 0f;
    public bool playRunningAnim = true;
}