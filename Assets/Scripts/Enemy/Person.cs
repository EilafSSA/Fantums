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
    private static System.Action OnBossRoomEntered; 
    
    // A permanent global flag to lock out any footsteps from new spawns
    private static bool hasBossRoomBeenEntered = false;

    public static void StopAllEnemyFootsteps()
    {
        hasBossRoomBeenEntered = true; // Set the permanent lock
        OnBossRoomEntered?.Invoke();   // Clear out existing ones
    }

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        OnBossRoomEntered += HandleBossRoomEntered;

        // CRUCIAL: Only start footsteps if the boss room has NOT been entered yet
        if (runLoopClip != null && footstepCoroutine == null && !hasBossRoomBeenEntered)
        {
            footstepCoroutine = StartCoroutine(PlayFootsteps());
        }
    }

    private void OnDisable()
    {
        OnBossRoomEntered -= HandleBossRoomEntered;
        StopFootstepLoop();
    }

    private IEnumerator PlayFootsteps()
    {
        while (true)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(runLoopClip, stepVolume);
            yield return new WaitForSeconds(stepRate);
        }
    }

    private void HandleBossRoomEntered()
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