using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("=== Audio ===")]
    [SerializeField] private AudioClip soundEffect; // deathSound
    [Range(0f, 1f)] [SerializeField] private float deathVolume = 1f; // Quick slider to adjust volume right from the inspector!
    
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

        // This creates a custom audio object that plays the sound cleanly, 
        // scales the volume exactly to your slider, and survives the enemy being destroyed.
        if (soundEffect != null)
        {
            GameObject tempAudioObj = new GameObject("TempDeathAudio");
            tempAudioObj.transform.position = transform.position;

            AudioSource source = tempAudioObj.AddComponent<AudioSource>();
            source.clip = soundEffect;
            source.volume = deathVolume;    // Assigned directly from the inspector slider
            source.spatialBlend = 0.0f;     // 0 = Pure 2D (loud everywhere), 1 = Strict 3D spatial sound. Change as desired.
            
            source.Play();

            // Safely disposes of the temporary audio object only after the sound finishes playing
            Destroy(tempAudioObj, soundEffect.length);
        }

        // Award score on kill
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddEnemyKillScore();
        }
        
        Destroy(gameObject);
    }
}