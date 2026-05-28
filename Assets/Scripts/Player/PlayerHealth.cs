using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("=== Health ===")]
    [SerializeField] private int maxHealth = 3;
    [SerializeField] private float invincibilityTime = 1.5f;
    [SerializeField] private float respawnDelay = 0.5f;

    [Header("=== Audio ===")]
    [SerializeField] private AudioSource playerSource;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;

    private Animator anim; // addedbyEilaf

    private int currentHealth;
    private bool isInvincible;
    private float invincibilityTimer;
    private SpriteRenderer sr;
    private Color originalColor;
    private Rigidbody2D rb;

    public static System.Action OnPlayerDied;

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
        anim = GetComponent<Animator>();

        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;

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

        //AUDIO LOGIC (Plays when hurt)
        if (playerSource != null && hurtSound != null)
        {
            playerSource.PlayOneShot(hurtSound);
        }

        //STREAMLINED SYSTEM LOGIC (Staging)
        Debug.Log($"[PlayerHealth] HP: {currentHealth}/{maxHealth}");
        
        if (CameraFollow.Instance != null)
        {
            CameraFollow.Instance.TriggerShake();
        }

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
        //NEW EVENT LOGIC (Staging)
        OnPlayerDied?.Invoke();

        //AUDIO LOGIC (Plays when dead)
        if (playerSource != null && deathSound != null)
        {
            playerSource.PlayOneShot(deathSound);
        }

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