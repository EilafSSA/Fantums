using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FinalBossMainBody : MonoBehaviour
{
    private Animator anim; 
    private AudioSource localAudioSource; // Found automatically at runtime

    [Header("=== Sound Clips ===")]
    [Tooltip("Both clips in this array will be played simultaneously")]
    [SerializeField] private AudioClip[] hurtClips = new AudioClip[2]; 
    [SerializeField] private AudioClip deathClip;

    private void Awake()
    { 
        anim = GetComponent<Animator>(); 
        localAudioSource = GetComponent<AudioSource>();

        // Fail-safe initialization to match your level 1 settings
        if (localAudioSource != null)
        {
            localAudioSource.playOnAwake = false;
        }
    }

    public void bodyHurt()
    {
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        PlayAllHurtSounds();
    }

    public void bodyDeath()
    {
        if (anim != null)
        {
            anim.SetTrigger("Death");
        }

        // 1. Play both hurt sounds layered together
        PlayAllHurtSounds();

        // 2. Overlay the unique death track immediately
        if (deathClip != null && localAudioSource != null)
        {
            localAudioSource.PlayOneShot(deathClip);
        }
    }

    private void PlayAllHurtSounds()
    {
        if (hurtClips == null || hurtClips.Length == 0 || localAudioSource == null) return;

        // Loop through and fire every clip inside the inspector array simultaneously via the local source
        foreach (AudioClip clip in hurtClips)
        {
            if (clip != null)
            {
                localAudioSource.PlayOneShot(clip);
            }
        }
    }
}