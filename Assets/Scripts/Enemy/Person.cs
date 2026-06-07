using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("=== Sound Clips ===")]
    [SerializeField] private AudioClip runLoopClip; 

    [Header("=== Footstep Settings ===")]
    [SerializeField] private float stepRate = 0.3f; 
    [SerializeField] private float minPitch = 0.85f; 
    [SerializeField] private float maxPitch = 1.15f; 

    private Coroutine footstepCoroutine;

    // The global kill-switch
    public static bool masterMute = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        // --- FIXED MIXER ROUTING ---
        // If our central UI manager exists, we steal its mixer group assignment 
        // and force this enemy's local AudioSource to pipe through the exact same channel.
        if (audioSource != null && UIAudioManager.Instance != null)
        {
            // Grabs the Audio Mixer Group assigned to your manager's SFX source
            AudioSource managerSFX = UIAudioManager.Instance.GetComponent<AudioSource>();
            if (managerSFX != null)
            {
                audioSource.outputAudioMixerGroup = managerSFX.outputAudioMixerGroup;
            }
        }
    }

    private void OnEnable()
    {
        // If the master mute has been flipped by the cutscene, block the loop instantly
        if (masterMute) return;

        if (runLoopClip != null && footstepCoroutine == null)
        {
            footstepCoroutine = StartCoroutine(PlayFootsteps());
        }
    }

    private void OnDisable()
    {
        StopFootstepLoop();
    }

    private IEnumerator PlayFootsteps()
    {
        while (true)
        {
            // Direct escape hatch if the mute gets flipped mid-loop
            if (masterMute)
            {
                StopFootstepLoop();
                yield break;
            }

            if (audioSource == null) yield break;
            
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            
            // We use the standard PlayOneShot without a hardcoded volume limit multiplier
            // because the Audio Mixer Group handle balances the amplitude now.
            audioSource.PlayOneShot(runLoopClip);
            yield return new WaitForSeconds(stepRate);
        }
    }

    public void ForceStopFootsteps()
    {
        StopFootstepLoop();
    }

    private void StopFootstepLoop()
    {
        if (footstepCoroutine != null)
        {
            StopCoroutine(footstepCoroutine);
            footstepCoroutine = null;
        }
        
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}