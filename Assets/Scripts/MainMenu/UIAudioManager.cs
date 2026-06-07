using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class UIAudioManager : MonoBehaviour
{
    public static UIAudioManager Instance { get; private set; }

    [Header("=== UI Sound Clips ===")]
    [SerializeField] private AudioClip clickSound;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioClip backOrCancelSound;

    [Header("=== Main Menu Music ===")]
    [SerializeField] private AudioClip mainMenuMusicTrack;
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.6f;

    private AudioSource sfxSource;
    private AudioSource musicSource;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        sfxSource = GetComponent<AudioSource>();
        sfxSource.spatialBlend = 0f;
        sfxSource.playOnAwake = false;

        // Automatically configure a secondary audio source purely for the music track
        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.spatialBlend = 0f;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
    }

    private void Start()
    {
        // Fire up the main menu theme immediately on scene launch
        PlayMenuMusic();
    }

    public void PlayMenuMusic()
    {
        if (mainMenuMusicTrack != null && musicSource != null)
        {
            musicSource.clip = mainMenuMusicTrack;
            musicSource.volume = musicVolume;
            musicSource.Play();
        }
    }

    // Direct command called by MainMenu when launching into Level 1
    public void FadeOutMenuMusic(float duration)
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(FadeMusicRoutine(duration));
        }
    }

    private IEnumerator FadeMusicRoutine(float duration)
    {
        if (musicSource == null) yield break;

        float startVolume = musicSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }

        musicSource.Stop();
    }

    public void PlayClick()
    {
        if (clickSound != null && sfxSource != null)
            sfxSource.PlayOneShot(clickSound);
    }

    public void PlayHover()
    {
        if (hoverSound != null && sfxSource != null)
            sfxSource.PlayOneShot(hoverSound);
    }

    public void PlayCancel()
    {
        if (backOrCancelSound != null && sfxSource != null)
            sfxSource.PlayOneShot(backOrCancelSound);
    }
}