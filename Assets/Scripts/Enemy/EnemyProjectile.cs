using UnityEngine;

// for some reason this was so damn hard (best notes askodfjaskldfj)
// this was suppsoed to e a simple proj script but idk why it was so hard to get it work right 
public class EnemyProjectile : MonoBehaviour
{
    [Header("=== Projectile Settings ===")]
    [SerializeField] private float speed = 7f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float knockbackForce = 6f;

    [Header("=== Visual ===")]
    [SerializeField] private bool rotateToDirection = true;

    [Header("=== Destroy On ===")]
    [Tooltip("Layers that destroy this projectile on contact (god help me)")]
    [SerializeField] private LayerMask groundLayer;

    private Animator anim; //addedbyEilaf
    private Rigidbody2D rb;
    private Vector2 direction = Vector2.right;
    private bool consumed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
    }

    private void Update() //addedbyEilaf
    {
        anim = GetComponent<Animator>(); //addedbyEilaf
    }

    public void Launch(Vector2 dir, float overrideSpeed = -1f)
    {
        direction = dir.normalized;
        float finalSpeed = overrideSpeed > 0f ? overrideSpeed : speed;
        
        if (rb != null)
            rb.linearVelocity = direction * finalSpeed;

        if (rotateToDirection)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        anim.SetTrigger("Shoot"); //addedbyEilaf
        HandleHit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        anim.SetTrigger("Shoot"); //addedbyEilaf
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider2D other)
    {
        if (consumed) return;

        if (other.GetComponent<EnemyProjectile>() != null) return;
        if (other.GetComponentInParent<EnemyProjectile>() != null) return;

        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        if (hp == null) hp = other.GetComponentInParent<PlayerHealth>();
        if (hp == null) hp = other.GetComponentInChildren<PlayerHealth>();

        if (hp != null)
        {
            hp.TakeDamage(damage);

            Rigidbody2D pRB = other.GetComponent<Rigidbody2D>();
            if (pRB == null) pRB = other.GetComponentInParent<Rigidbody2D>();

            if (pRB != null)
            {
                Vector2 knockDir = new Vector2(direction.x, 0.5f).normalized;
                pRB.linearVelocity = Vector2.zero;
                pRB.AddForce(knockDir * knockbackForce, ForceMode2D.Impulse);
            }

            consumed = true;
            Destroy(gameObject);
            return;
        }

        if (((1 << other.gameObject.layer) & groundLayer.value) != 0)
        {
            consumed = true;
            Destroy(gameObject);
            return;
        }

    }
}