using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class EnemyAudio : MonoBehaviour
{
    private AudioSource audioSource;

    [Header("=== Sound Clips ===")]
    [SerializeField] private AudioClip runLoopClip; 

    [Header("=== Footstep Settings ===")]
    [SerializeField, Range(0f, 1f)] private float stepVolume = 0.4f; 
    [SerializeField] private float stepRate = 0.3f; 
    [SerializeField] private float minPitch = 0.85f; 
    [SerializeField] private float maxPitch = 1.15f; 

    private Coroutine footstepCoroutine;

    // The global kill-switch
    public static bool masterMute = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // REMOVED: masterMute = false; <-- This was resetting the mute for everyone!
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
            audioSource.PlayOneShot(runLoopClip, stepVolume);
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