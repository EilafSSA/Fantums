using UnityEngine;

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
    [Tooltip("Layers that destroy this projectile on contact")]
    [SerializeField] private LayerMask groundLayer;

    [Header("=== Audio ===")]
    [SerializeField] private AudioClip popSound;
    [Range(0f, 1f)] [SerializeField] private float popVolume = 0.8f; // Easy volume tweak directly from Inspector

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
        HandleHit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider2D other)
    {
        if (consumed) return;

        // Skip hitting fellow projectiles
        if (other.GetComponent<EnemyProjectile>() != null || other.GetComponentInParent<EnemyProjectile>() != null) 
            return;

        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        if (hp == null) hp = other.GetComponentInParent<PlayerHealth>();
        if (hp == null) hp = other.GetComponentInChildren<PlayerHealth>();

        // 1. HIT PLAYER LOGIC
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

            TriggerPopAudio();
            consumed = true;
            Destroy(gameObject);
            return;
        }

        // 2. HIT GROUND/WALL LOGIC
        if (((1 << other.gameObject.layer) & groundLayer.value) != 0)
        {
            TriggerPopAudio();
            consumed = true;
            Destroy(gameObject);
            return;
        }
    }

    private void TriggerPopAudio()
    {
        if (popSound != null)
        {
            // Create independent temp sound object so 3D camera distance attenuation doesn't crush it
            GameObject tempAudioObj = new GameObject("TempPopAudio");
            tempAudioObj.transform.position = transform.position;

            AudioSource source = tempAudioObj.AddComponent<AudioSource>();
            source.clip = popSound;
            source.volume = popVolume;
            source.spatialBlend = 0.0f; // Force to 2D space so it sounds loud and direct everywhere
            source.Play();

            Destroy(tempAudioObj, popSound.length);
        }
    }
}