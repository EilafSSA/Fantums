using UnityEngine;

// floaty ghost. no gravity. patrols between two poi
// and chases the player if spotted inside its forward cone
public class GhostEnemy : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private Transform player;

    [Header("=== Movement ===")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 2.6f;
    [SerializeField] private float bobAmplitude = 0.35f;
    [SerializeField] private float bobFrequency = 1.4f;

    [Header("=== Patrol ===")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [SerializeField] private float arrivalThreshold = 0.25f;
    [SerializeField] private float lookBackDuration = 1.0f;

    [Header("=== Cone Vision ===")]
    [Tooltip("Total cone angle in degrees. 60 covers ~30 above and below facing direction.")]
    [SerializeField, Range(10f, 180f)] private float viewAngle = 60f;
    [SerializeField] private float viewDistance = 6f;
    [SerializeField] private LayerMask sightBlockers;

    [Header("=== Chase Behavior ===")]
    [SerializeField] private float loseSightTime = 2f;
    [SerializeField] private float chaseStopDistance = 0.3f;

    [Header("=== Facing ===")] //this is to avoid the flickering it does from left to right or well an attept 
    [SerializeField] private float facingDeadzone = 0.75f;
    [SerializeField] private float flipCooldown = 0.25f;

    [Header("=== Animation ===")] //for eliaf, if u want to add the anim for it moving 
    [SerializeField] private Animator anim; //but u dont have to if its not necc then remove this whole section and the anim reference

    private Rigidbody2D rb;
    private Vector3 baseScale;
    private Transform currentTarget;
    private bool facingRight = true;
    private float lookBackTimer;
    private float loseSightTimer;
    private float bobSeed;
    private float nextFlipAllowedTime;

    private enum State { Patrol, LookBack, Chase }
    private State state = State.Patrol;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        baseScale = transform.localScale;
        bobSeed = Random.value * 100f;
    }

    private void Start()
    {
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (pointA != null && pointB != null)
        {
            currentTarget = pointB;
            FaceTowards(currentTarget.position.x);
        }
    }

    private void FixedUpdate()
    {
        bool canSee = CanSeePlayer();

        if (canSee)
        {
            loseSightTimer = loseSightTime;
            state = State.Chase;
        }
        else if (state == State.Chase)
        {
            loseSightTimer -= Time.fixedDeltaTime;
            if (loseSightTimer <= 0f)
                state = State.Patrol;
        }

        switch (state)
        {
            case State.Patrol: DoPatrol(); break;
            case State.LookBack: DoLookBack(); break;
            case State.Chase: DoChase(); break;
        }

        ApplyBob();

        if (anim != null)
            anim.SetBool("IsChasing", state == State.Chase);
    }


    private void DoPatrol()
    {
        if (currentTarget == null || pointA == null || pointB == null)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float dirX = Mathf.Sign(currentTarget.position.x - transform.position.x);
        rb.linearVelocity = new Vector2(dirX * patrolSpeed, rb.linearVelocity.y);
        FaceTowards(currentTarget.position.x);

        if (Mathf.Abs(transform.position.x - currentTarget.position.x) <= arrivalThreshold)
        {
            state = State.LookBack;
            lookBackTimer = lookBackDuration;
            // flip immediately, bypass cooldown
            facingRight = !facingRight;
            ApplyFacing();
            nextFlipAllowedTime = Time.time + flipCooldown;
            rb.linearVelocity = Vector2.zero;
        }
    }

    private void DoLookBack()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        lookBackTimer -= Time.fixedDeltaTime;

        if (lookBackTimer <= 0f)
        {
            currentTarget = (currentTarget == pointA) ? pointB : pointA;
            FaceTowards(currentTarget.position.x);
            state = State.Patrol;
        }
    }

    private void DoChase()
    {
        if (player == null) return;

        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;

        // don't jitter when basically on top of the player
        if (toPlayer.magnitude < chaseStopDistance)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 dir = toPlayer.normalized;
        rb.linearVelocity = dir * chaseSpeed;
        FaceTowards(player.position.x);
    }


    private void ApplyBob()
    {
        float wave = Mathf.Sin((Time.time + bobSeed) * bobFrequency * Mathf.PI) * bobAmplitude;
        Vector2 v = rb.linearVelocity;
        v.y += wave * Time.fixedDeltaTime * 8f;
        rb.linearVelocity = v;
    }

    private bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector2 toPlayer = (Vector2)player.position - (Vector2)transform.position;
        float dist = toPlayer.magnitude;
        if (dist > viewDistance) return false;

        Vector2 forward = facingRight ? Vector2.right : Vector2.left;
        float angleToPlayer = Vector2.Angle(forward, toPlayer.normalized);
        if (angleToPlayer > viewAngle * 0.5f) return false;

        if (sightBlockers.value != 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, toPlayer.normalized, dist, sightBlockers);
            if (hit.collider != null) return false;
        }

        return true;
    }

    private void FaceTowards(float targetX)
    {
        if (Time.time < nextFlipAllowedTime) return;

        float delta = targetX - transform.position.x;

        if (Mathf.Abs(delta) < facingDeadzone) return;

        bool wantRight = delta > 0f;
        if (wantRight == facingRight) return;

        facingRight = wantRight;
        ApplyFacing();
        nextFlipAllowedTime = Time.time + flipCooldown;
    }

    private void ApplyFacing()
    {
        Vector3 s = baseScale;
        s.x = Mathf.Abs(baseScale.x) * (facingRight ? 1f : -1f);
        transform.localScale = s;
    }

    private void OnDrawGizmosSelected()
    {
        if (pointA != null && pointB != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(pointA.position, pointB.position);
            Gizmos.color = Color.red; Gizmos.DrawWireSphere(pointA.position, 0.2f);
            Gizmos.color = Color.green; Gizmos.DrawWireSphere(pointB.position, 0.2f);
        }

        Vector3 origin = transform.position;
        Vector3 forward = (Application.isPlaying
            ? (facingRight ? Vector3.right : Vector3.left)
            : (transform.localScale.x >= 0 ? Vector3.right : Vector3.left));

        float half = viewAngle * 0.5f;
        Vector3 a = Quaternion.Euler(0f, 0f, half) * forward * viewDistance;
        Vector3 b = Quaternion.Euler(0f, 0f, -half) * forward * viewDistance;

        Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.9f);
        Gizmos.DrawLine(origin, origin + a);
        Gizmos.DrawLine(origin, origin + b);
        Gizmos.DrawLine(origin + a, origin + b);

        Gizmos.color = new Color(1f, 0.4f, 0.8f, 0.5f);
        Gizmos.DrawWireSphere(origin, viewDistance);
    }
}