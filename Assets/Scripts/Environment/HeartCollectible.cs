using UnityEngine;

public class HeartCollectible : MonoBehaviour
{
    [Header("=== Settings ===")]
    [SerializeField] private int healAmount = 1;
    [SerializeField] private float pickupRadius = 1.5f;
    
    [Header("=== Audio ===")]
    [SerializeField] private AudioClip collectSound; 
    
    private Animator anim; // addedbyEilaf
    private bool isCollected = false; // Added to prevent double-triggering sound or healing

    private void Awake()
    {
        anim = GetComponent<Animator>(); // addedbyEilaf
    }

    private void Update()
    {
        if (isCollected) return;

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance <= pickupRadius)
            {
                ProcessHeal(player);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        ProcessHeal(collision.gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        ProcessHeal(collision.gameObject);
    }

    private void ProcessHeal(GameObject otherObj)
    {
        if (isCollected) return;

        PlayerHealth health = otherObj.GetComponent<PlayerHealth>();

        if (health == null)
        {
            health = otherObj.GetComponentInParent<PlayerHealth>();
        }

        if (health == null)
        {
            health = otherObj.GetComponentInChildren<PlayerHealth>();
        }

        if (health == null)
        {
            PlayerController pc = otherObj.GetComponent<PlayerController>();
            if (pc == null) pc = otherObj.GetComponentInParent<PlayerController>();
            if (pc == null) pc = otherObj.GetComponentInChildren<PlayerController>();
            if (pc != null)
            {
                health = pc.GetComponent<PlayerHealth>();
            }
        }

        if (health == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null && (otherObj == player || otherObj.transform.IsChildOf(player.transform)))
            {
                health = player.GetComponent<PlayerHealth>();
            }
        }

        // Only heal and play sound if the player is actually missing health
        if (health != null && health.GetCurrentHealth() < health.GetMaxHealth())
        {
            isCollected = true; // Instantly lock it so it can't trigger twice in the same frame
            
            health.Heal(healAmount);

            // --- FIXED MIXER ROUTING ---
            // Instead of spawning an unrouted clip in the wild, we play it through our Mixer-linked manager
            if (collectSound != null && UIAudioManager.Instance != null)
            {
                UIAudioManager.Instance.PlayOneShotSFX(collectSound);
            }

            if (anim != null)
            {
                anim.SetTrigger("Sway");
            }

            Destroy(gameObject, 0.85f);
        }
    }
}