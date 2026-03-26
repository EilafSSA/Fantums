using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("=== Health ===")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibilityTime = 1.5f;
    [SerializeField] private float respawnDelay = 0.5f;

    private int currentHealth;
    private bool isInvincible;
    private float invincibilityTimer;
    private SpriteRenderer sr;
    private Color originalColor;
    private Rigidbody2D rb;

    private void Awake()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        if (sr != null)
            originalColor = sr.color;
    }

    private void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;

            // flicker effect - should be done in shader ideally, but good temporary solution for our demo @eliaf since u liked the flicker :3
            // pingpong alpha between 0 and 1 to create a flicker effect = can also add to enemies to give them hit feedback aswell + knockback later
            if (sr != null)
            {
                float alpha = Mathf.PingPong(Time.time * 10f, 1f);
                sr.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            }

            if (invincibilityTimer <= 0f)
            {
                isInvincible = false;
                if (sr != null)
                    sr.color = originalColor;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log($"[PlayerHealth] HP: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        isInvincible = true;
        invincibilityTimer = invincibilityTime;
    }

    private void Die()
    {
        Debug.Log("[PlayerHealth] Player Died!");
        StartCoroutine(RespawnRoutine());
    }

    private System.Collections.IEnumerator RespawnRoutine()
    {
        isInvincible = true;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (sr != null)
            sr.enabled = false;

        yield return new WaitForSeconds(respawnDelay);

        Vector3 respawnPos = GameManager.Instance != null
            ? GameManager.Instance.GetRespawnPosition()
            : new Vector3(-8f, -2.5f, 0f);

        transform.position = respawnPos;

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        //
        currentHealth = maxHealth;

        if (sr != null)
        {
            sr.enabled = true;
            sr.color = originalColor;
        }

        invincibilityTimer = invincibilityTime;
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}