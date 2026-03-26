using UnityEngine;
// probably really good to also incldue this in projectiles I think, just need to fix the projectiles so they do not collide with the player since right now the player can literally move the bullet and since the bullet needs a one second timer in the hit box to damage the player maybe we can just take functions from this then cause for sure 1 second makes no sense for a bullet, must be instnat but refrence for future codie.
public class EnemyHitbox : MonoBehaviour
{
    [SerializeField] private int contactDamage = 1;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float damageCooldown = 1f;

    private float cooldownTimer;

    private void Update()
    {
        if (cooldownTimer > 0f)
            cooldownTimer -= Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamagePlayer(other);
    }

    private void TryDamagePlayer(Collider2D other)
    {
       
        GameObject root = other.transform.root.gameObject;
        bool isPlayer = other.CompareTag("Player") || root.CompareTag("Player");
        if (!isPlayer) return;

        if (cooldownTimer > 0f) return;

        
        PlayerHealth hp = other.GetComponent<PlayerHealth>();
        if (hp == null) hp = other.GetComponentInParent<PlayerHealth>();

        if (hp != null)
        {
            hp.TakeDamage(contactDamage);
            Debug.Log($"[EnemyHitbox] HIT PLAYER for {contactDamage} damage!");
        }
        else
        {
            Debug.LogWarning("[EnemyHitbox] Could not find PlayerHealth on player hierarchy!");
            return;
        }

        // nockback
        Rigidbody2D playerRB = other.GetComponent<Rigidbody2D>();
        if (playerRB == null) playerRB = other.GetComponentInParent<Rigidbody2D>();

        if (playerRB != null)
        {
            Vector2 knockDir = (root.transform.position - transform.parent.position).normalized;
            knockDir.y = 0.5f;
            playerRB.linearVelocity = Vector2.zero;
            playerRB.AddForce(knockDir.normalized * knockbackForce, ForceMode2D.Impulse);
        }

        cooldownTimer = damageCooldown;
    }
}