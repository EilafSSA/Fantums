using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerDetection : MonoBehaviour
{
    [SerializeField] private AudioClip gateSound;
    private Animator anim;
    private bool hasTriggered = false; 

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 1. Verify it's the player and we haven't fired yet
        if (other.CompareTag("Player") && !hasTriggered)
        {
            // 2. Lock it immediately so it never triggers again
            hasTriggered = true; 

            if (anim != null) 
            {
                anim.SetTrigger("Sway");
            }

            // 3. Direct Runtime Call: Find the live Instance exactly when needed
            if (AudioManager.Instance != null)
            {
                if (gateSound != null)
                {
                    AudioManager.Instance.PlayGateSound(gateSound, transform.position);
                }
                else
                {
                    //Debug.LogWarning($"PlayerDetection on {gameObject.name}: Gate Sound clip is missing in the Inspector!");
                }
            }
            else
            {
                //Debug.LogError("PlayerDetection: Could not find a live AudioManager Instance!");
            }
        }
    }

    
        
    
}