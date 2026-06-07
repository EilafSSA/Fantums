using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("=== Audio ===")]
    [SerializeField] private AudioClip soundEffect; // deathSound
    
    [Header("=== Health Settings ===")]
    [SerializeField] private int maxHealth = 3;
    
    private int currentHealth;
    private Animator anim; 

    private void Awake()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>(); // Performance fix: Cached once safely on initialization
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log($"Enemy HP: {currentHealth}/{maxHealth}");
        
        if (anim != null)
        {
            anim.SetTrigger("IsHurt"); 
        }

        StartCoroutine(FlashDamage());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashDamage()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color original = sr.color;
            sr.color = Color.white;
            yield return new WaitForSeconds(0.1f);
            sr.color = original;
        }
    }

    private void Die()
    {
        Debug.Log("Enemy Died!");

        // --- FIXED MIXER ROUTING (SPATIAL 3D) ---
        // We cut the manual instantiation block entirely.
        // Handing the clip off to the manager guarantees it survives this object's destruction
        // and instantly respects your central options menu volume settings.
        if (soundEffect != null && UIAudioManager.Instance != null)
        {
            UIAudioManager.Instance.PlaySpatialSFX(soundEffect, transform.position);
        }

        // Award score on kill
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddEnemyKillScore();
        }
        
        Destroy(gameObject);
    }
}