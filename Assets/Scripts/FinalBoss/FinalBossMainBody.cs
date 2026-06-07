using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FinalBossMainBody : MonoBehaviour
{
    private Animator anim; 
    private AudioSource audioSource;

    [Header("=== Boss Sound Settings ===")]
    [SerializeField, Range(0f, 1f)] private float bossSFXVolume = 0.6f;

    [Header("=== Sound Clips ===")]
    [Tooltip("Add exactly 2 different hurt clips here")]
    [SerializeField] private AudioClip[] hurtClips = new AudioClip[2]; 
    [SerializeField] private AudioClip deathClip;

    private void Awake()
    { 
        anim = GetComponent<Animator>(); 
        audioSource = GetComponent<AudioSource>();

        //  Ensure the AudioSource is set up for SFX and not music
        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 0f; // 2D Full Screen Volume
        }
    }

    public void bodyHurt()
    {
        if (anim != null)
        {
            anim.SetTrigger("Hurt");
        }

        PlayRandomHurtSound();
    }

    public void bodyDeath()
    {
        if (anim != null)
        {
            anim.SetTrigger("Death");
        }

        // 1. Play a hurt sound variations during death crunch
        PlayRandomHurtSound();

        // 2. Overlay the unique explosive/death sound on top immediately
        if (deathClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathClip, bossSFXVolume);
        }
    }

    private void PlayRandomHurtSound()
    {
        if (audioSource == null || hurtClips == null || hurtClips.Length == 0) return;

        // Pick randomly between the clips added to the inspector array
        int randomIndex = Random.Range(0, hurtClips.Length);
        AudioClip selectedClip = hurtClips[randomIndex];

        if (selectedClip != null)
        {
            audioSource.PlayOneShot(selectedClip, bossSFXVolume);
        }
    }
}