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

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.spatialBlend = 0f;
        musicSource.loop = true;
        musicSource.playOnAwake = false;
    }

    private void Start()
    {
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

    // --- 2D MIXER ROUTED METHOD ---
    public void PlayOneShotSFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    // --- 3D SPATIAL MIXER ROUTED METHOD (FIXES YOUR COMPILER ERROR) ---
    public void PlaySpatialSFX(AudioClip clip, Vector3 position, float spatialBlend = 1f)
    {
        if (clip == null) return;

        GameObject tempAudioObj = new GameObject("TempSpatialAudio");
        tempAudioObj.transform.position = position;

        AudioSource source = tempAudioObj.AddComponent<AudioSource>();
        source.clip = clip;
        source.spatialBlend = spatialBlend; 
        source.minDistance = 2f;
        source.maxDistance = 15f;
        source.rolloffMode = AudioRolloffMode.Logarithmic;

        if (sfxSource != null)
        {
            source.outputAudioMixerGroup = sfxSource.outputAudioMixerGroup;
        }

        source.Play();
        Destroy(tempAudioObj, clip.length);
    }

    // Old UI methods kept for safety/compatibility
    public void PlayClick() { if (clickSound != null && sfxSource != null) sfxSource.PlayOneShot(clickSound); }
    public void PlayHover() { if (hoverSound != null && sfxSource != null) sfxSource.PlayOneShot(hoverSound); }
    public void PlayCancel() { if (backOrCancelSound != null && sfxSource != null) sfxSource.PlayOneShot(backOrCancelSound); }
}