using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AudioManager>();
                if (_instance == null)
                {
                    // Looks inside your Assets/Resources folder to clone the configured prefab automatically
                    AudioManager prefab = Resources.Load<AudioManager>("AudioManager");
                    if (prefab != null)
                    {
                        _instance = Instantiate(prefab);
                        _instance.name = "AudioManager (Auto-Created)";
                        DontDestroyOnLoad(_instance.gameObject);
                    }
                    else
                    {
                        GameObject singleton = new GameObject("AudioManager (Fallback)");
                        _instance = singleton.AddComponent<AudioManager>();
                        DontDestroyOnLoad(singleton);
                    }
                }
            }
            return _instance;
        }
    }

    [Header("=== Music Settings ===")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip victoryClip;
    [SerializeField, Range(0f, 1f)] private float maxMusicVolume = 0.5f;
    [SerializeField] private AudioClip defaultLevel2Music; 

    [Header("=== Arm Sounds ===")]
    [SerializeField] private AudioClip warningClip;
    [SerializeField] private AudioClip wooshClip;
    [SerializeField] private float wooshDelay = 0.2f;
    [SerializeField, Range(0f, 1f)] private float armVolume = 0.5f;

    [Header("=== UI Sounds ===")]
    [SerializeField] private AudioSource uiSource;
    [SerializeField] private AudioClip transitionClip;

    private bool isFading = false;

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeComponents();
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeComponents()
    {
        if (musicSource == null) musicSource = GetComponent<AudioSource>();
        if (uiSource == null && transform.childCount > 0) uiSource = GetComponentInChildren<AudioSource>();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeComponents();

        // Match your scene asset name 'Level2' exactly as seen in your project folder
        if (scene.name == "Level2") 
        {
            if (defaultLevel2Music != null)
            {
                PlayBossMusic(defaultLevel2Music, 0.2f, 1.0f);
            }
        }
    }

    // --- Boss & Victory Music ---
    public void SwitchToBossMusic(AudioClip bossTrack, float fadeOutTime = 0.5f, float fadeInTime = 1.0f)
    {
        if (bossTrack != null)
        {
            PlayBossMusic(bossTrack, fadeOutTime, fadeInTime);
        }
    }
    public void PlayBossMusic(AudioClip newClip, float fadeOut = 0.2f, float fadeIn = 1.0f)
    {
        SwitchMusic(newClip, fadeOut, fadeIn);
    }

    public void PlayVictoryMusic(float fadeOut = 0.5f, float fadeIn = 2.0f)
    {
        // 1. Force looping OFF right here so the victory stinger only plays once!
        if (musicSource != null) musicSource.loop = false;

        if (victoryClip != null) SwitchMusic(victoryClip, fadeOut, fadeIn);
    }

    public void SwitchMusic(AudioClip newClip, float fadeOut, float fadeIn)
    {
        if (newClip == null) return;
        if (isFading) StopAllCoroutines();
        StartCoroutine(CrossfadeMusic(newClip, fadeOut, fadeIn));
    }

    private IEnumerator CrossfadeMusic(AudioClip newClip, float fadeOut, float fadeIn)
    {
        isFading = true;

        if (musicSource != null)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;
            while (elapsed < fadeOut)
            {
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOut);
                elapsed += Time.deltaTime;
                yield return null;
            }
            musicSource.Stop();
            
            // 2. DYNAMIC LOOP CHECK: 
            // If the clip about to play is the victory stinger, do NOT loop it.
            // Otherwise, it's level background music or boss music, so it MUST loop.
            if (newClip == victoryClip)
            {
                musicSource.loop = false;
            }
            else
            {
                musicSource.loop = true;
            }

            musicSource.clip = newClip;
            musicSource.Play();
        }

        float fadeInElapsed = 0f;
        while (fadeInElapsed < fadeIn)
        {
            if (musicSource != null) musicSource.volume = Mathf.Lerp(0f, maxMusicVolume, fadeInElapsed / fadeIn);
            fadeInElapsed += Time.deltaTime;
            yield return null;
        }

        if (musicSource != null) musicSource.volume = maxMusicVolume;
        isFading = false;
    }

    // --- SFX Logic ---
    public void PlayTransitionSound()
    {
        if (uiSource != null && transitionClip != null) uiSource.PlayOneShot(transitionClip);
    }

    
    public void PlayGateSound(AudioClip clip, Vector3 position)
    {
        // Bypass 3D spacing completely and play it through your UI/SFX source
        if (uiSource != null && clip != null) 
        {
            uiSource.PlayOneShot(clip);
        }
        else if (clip != null)
        {
            // Fallback: If uiSource isn't assigned, play it globally at the camera's plane
            Vector3 cameraPlanePosition = new Vector3(position.x, position.y, Camera.main.transform.position.z + 1f);
            AudioSource.PlayClipAtPoint(clip, cameraPlanePosition);
        }
    }

    public void PlayArmWarningSounds(Vector3 position)
    {
        StartCoroutine(PlayDelayedAudio(position));
    }

    private IEnumerator PlayDelayedAudio(Vector3 position)
    {
        GameObject tempAudio = new GameObject("ArmWarningSound");
        tempAudio.transform.position = position;
        AudioSource source = tempAudio.AddComponent<AudioSource>();
        source.volume = armVolume;
        source.spatialBlend = 1.0f;
        
        if (warningClip != null) source.PlayOneShot(warningClip);
        yield return new WaitForSeconds(wooshDelay);
        if (wooshClip != null) source.PlayOneShot(wooshClip);
        
        Destroy(tempAudio, 2f);
    }
}