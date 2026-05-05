using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ShadowBossHitbox : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private ShadowBoss boss;
    
    [Header("=== Contact Damage ===")]
    [SerializeField] private bool dealContactDamage = true;
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float damageCooldown = 1f;
    
    private float lastDamageTime = -999f;

    private void Awake()
    {
        if (boss == null)
        {
            boss = GetComponent<ShadowBoss>();
            if (boss == null)
            {
                boss = GetComponentInParent<ShadowBoss>();
            }
        }
        
        if (boss == null)
        {
            Debug.LogError("ShadowBossHitbox: No ShadowBoss component found!");
        }
    }

    public void TakeDamage(int damage)
    {
        if (boss != null)
        {
            boss.TakeDamage(damage);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (dealContactDamage)
        {
            TryDamagePlayer(other);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (dealContactDamage)
        {
            TryDamagePlayer(other);
        }
    }

    private void TryDamagePlayer(Collider2D other)
    {
        if (Time.time - lastDamageTime < damageCooldown) return;

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth == null)
            playerHealth = other.GetComponentInParent<PlayerHealth>();
            
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(contactDamage);
            lastDamageTime = Time.time;
            
            Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
            if (playerRb == null)
                playerRb = other.GetComponentInParent<Rigidbody2D>();
                
            if (playerRb != null)
            {
                Vector2 knockbackDir = (other.transform.position - transform.position).normalized;
                knockbackDir.y = Mathf.Max(0.3f, knockbackDir.y);
                knockbackDir.Normalize();
                playerRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
            }
        }
    }
}
